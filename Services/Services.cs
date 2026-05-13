using Lanchonete.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lanchonete.Services
{
   
    public static class Formatador
    {
        public static readonly CultureInfo Cultura = new CultureInfo("pt-BR");
        public const int LARGURA = 56;

        public static string Preco(decimal valor) => valor.ToString("C", Cultura);

        // Monta uma linha tipo:
        //  "X-Burger ............................. R$ 22,00"
      
       
        public static string LinhaComPontos(string esquerda, string direita, int largura = LARGURA)
        {
            int pontos = Math.Max(3, largura - esquerda.Length - direita.Length - 2);
            return $"{esquerda} {new string('.', pontos)} {direita}";
        }
    }


    /// Gerencia o pedido: agrega produtos, calcula totais, aplica desconto.

    public class Pedido
    {
        private readonly Cliente _cliente;
        private readonly List<Produto> _itens = new List<Produto>();
        private readonly int _numero;

        private const decimal TAXA_ENTREGA = 7.00m;
        private const decimal LIMITE_DESCONTO = 50.00m;
        private const decimal PERCENTUAL_DESCONTO = 0.10m;

        public Cliente Cliente => _cliente;
        public int Numero => _numero;
        // ENCAPSULAMENTO: expõe a lista como somente-leitura 
        // só consegue ler, não consegue alterar diretamente.
        public IReadOnlyList<Produto> Itens => _itens.AsReadOnly();

        public Pedido(Cliente cliente)
        {
            _cliente = cliente ?? throw new ArgumentNullException(nameof(cliente));
            _numero = new Random().Next(100, 1000);
        }

        public void AdicionarProduto(Produto p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            _itens.Add(p);
        }

        public bool RemoverProduto(int index)
        {
            if (index < 0 || index >= _itens.Count) return false;
            _itens.RemoveAt(index);
            return true;
        }

        public decimal CalcularSubtotal()
        {
            decimal subtotal = 0;
            // POLIMORFISMO: percorremos uma lista de Produto, mas cada item, pode ser Lanche, Bebida, Sobremesa ou Combo. Não importa  o preco funciona para todos porque todos herdam de Produto.
            foreach (var item in _itens) subtotal += item.Preco;
            return subtotal;
        }

        public decimal CalcularDesconto()
        {
            var sub = CalcularSubtotal();
            return sub > LIMITE_DESCONTO ? sub * PERCENTUAL_DESCONTO : 0m;
        }

        public decimal CalcularTotal()
        {
            return CalcularSubtotal() - CalcularDesconto() + TAXA_ENTREGA;
        }

        public void ExibirResumo()
        {
            decimal sub = CalcularSubtotal();
            decimal desc = CalcularDesconto();
            decimal total = CalcularTotal();
            int tempoEstimado = new Random().Next(30, 51);

            Console.WriteLine();
            Console.WriteLine("==============================================================");
            Console.WriteLine($"                      PEDIDO #{_numero:D3}");
            Console.WriteLine("==============================================================");
            Console.WriteLine($"  Cliente:    {_cliente.Nome}");
            Console.WriteLine($"  Telefone:   {_cliente.Telefone}");
            Console.WriteLine($"  Endereço:   {_cliente.Endereco.Logradouro}, {_cliente.Endereco.Numero}" +
                              (string.IsNullOrWhiteSpace(_cliente.Endereco.Complemento) ? "" : $" - {_cliente.Endereco.Complemento}"));
            Console.WriteLine($"              {_cliente.Endereco.Bairro}, {_cliente.Endereco.Cidade}/{_cliente.Endereco.Uf}");
            Console.WriteLine($"              CEP: {_cliente.Endereco.Cep}");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("  ITENS");
            Console.WriteLine("--------------------------------------------------------------");

            foreach (var item in _itens)
            {
                Console.WriteLine("  " + Formatador.LinhaComPontos($"1x  {item.Nome}", Formatador.Preco(item.Preco)));
                Console.WriteLine($"      {item.Detalhe()}");
                Console.WriteLine();
            }

            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("  " + Formatador.LinhaComPontos("Subtotal", Formatador.Preco(sub)));
            if (desc > 0)
                Console.WriteLine("  " + Formatador.LinhaComPontos("Desconto 10% (acima de R$ 50)", "-" + Formatador.Preco(desc)));
            Console.WriteLine("  " + Formatador.LinhaComPontos("Taxa de entrega", Formatador.Preco(TAXA_ENTREGA)));
            Console.WriteLine("  " + Formatador.LinhaComPontos("TOTAL", Formatador.Preco(total)));
            Console.WriteLine("==============================================================");
            Console.WriteLine($"  Pedido confirmado. Tempo estimado: {tempoEstimado} minutos.");
            Console.WriteLine();
        }
    }


    // Consulta endereços na API pública ViaCEP (https://viacep.com.br).
    // Encapsula toda a complexidade de HTTP e JSON: quem usa só chama ConsultarCepAsync e recebe um objeto Endereco pronto.

    public class ViaCepService
    {
       
        private static readonly HttpClient _http = new HttpClient();

        public async Task<Endereco> ConsultarCepAsync(string cep)
        {
            // Aceita "01001-000", "01001000" etc 
            var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
            if (cepLimpo.Length != 8)
                throw new ArgumentException("O CEP deve conter 8 dígitos.");

            try
            {
                var url = $"https://viacep.com.br/ws/{cepLimpo}/json/";
                var json = await _http.GetStringAsync(url);

                // ViaCEP retorna { "erro": true } para CEP inexistente
                if (json.Contains("\"erro\""))
                    throw new Exception("CEP não encontrado.");

                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resp = JsonSerializer.Deserialize<ViaCepResposta>(json, opcoes);

                return new Endereco
                {
                    Cep = resp.Cep,
                    Logradouro = resp.Logradouro,
                    Bairro = resp.Bairro,
                    Cidade = resp.Localidade,
                    Uf = resp.Uf,
                    Complemento = resp.Complemento
                };
            }
            catch (HttpRequestException)
            {
                throw new Exception("Falha de conexão. Verifique sua internet.");
            }
        }

        // Classe interna privada: o "contrato" do JSON do ViaCEP fica escondido, quem usa o serviço não precisa saber dos detalhes.
        private class ViaCepResposta
        {
            [JsonPropertyName("cep")]         public string Cep { get; set; }
            [JsonPropertyName("logradouro")]  public string Logradouro { get; set; }
            [JsonPropertyName("complemento")] public string Complemento { get; set; }
            [JsonPropertyName("bairro")]      public string Bairro { get; set; }
            [JsonPropertyName("localidade")]  public string Localidade { get; set; }
            [JsonPropertyName("uf")]          public string Uf { get; set; }
        }
    }

  
    /// Cardápio fixo da lanchonete. Centraliza os produtos disponíveis
  
    public static class Cardapio
    {
        public static List<Lanche> Lanches { get; } = new List<Lanche>
        {
            new Lanche("X-Burger",           22.00m, new List<string>{ "pão brioche", "hambúrguer 150g", "queijo cheddar" }),
            new Lanche("X-Salada",           25.00m, new List<string>{ "pão brioche", "hambúrguer 150g", "cheddar", "alface", "tomate" }),
            new Lanche("X-Bacon",            28.00m, new List<string>{ "pão brioche", "hambúrguer 150g", "cheddar", "bacon crocante" }),
            new Lanche("X-Tudo",             35.00m, new List<string>{ "pão brioche", "hambúrguer duplo", "cheddar", "bacon", "ovo", "presunto" }),
            new Lanche("Hot Dog Especial",   18.00m, new List<string>{ "pão de hot dog", "salsicha", "batata palha", "milho", "molho da casa" }),
            new Lanche("Cheeseburger Duplo", 30.00m, new List<string>{ "pão americano", "dois hambúrgueres", "cheddar duplo", "cebola caramelizada" })
        };

        public static List<Bebida> Bebidas { get; } = new List<Bebida>
        {
            new Bebida("Coca-Cola",              7.00m,  350),
            new Bebida("Coca-Cola Grande",       10.00m, 600),
            new Bebida("Suco de Laranja",        12.00m, 400),
            new Bebida("Água Mineral",           5.00m,  500),
            new Bebida("Milkshake de Chocolate", 18.00m, 500)
        };

        public static List<Sobremesa> Sobremesas { get; } = new List<Sobremesa>
        {
            new Sobremesa("Pudim de Leite",      10.00m, "tradicional, com calda de caramelo"),
            new Sobremesa("Brownie com Sorvete", 15.00m, "brownie quente coberto com bola de sorvete de creme"),
            new Sobremesa("Sorvete",             12.00m, "duas bolas, sabores à sua escolha"),
            new Sobremesa("Petit Gateau",        18.00m, "bolinho de chocolate com recheio cremoso")
        };
    }
}