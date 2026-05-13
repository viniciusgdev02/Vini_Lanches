using Lanchonete.Models;
using Lanchonete.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

class Program
{
    static async Task Main()
    {
        // Habilita acentos e símbolos no console do Windows
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine("                         VINI LANCHES");
        Console.WriteLine("==============================================================");
        Console.WriteLine();

        // Cadastra cliente 
        var cliente = await CadastrarClienteAsync();
        var pedido = new Pedido(cliente);

        Console.WriteLine();
        Console.WriteLine($"Tudo certo, {cliente.Nome.Split(' ')[0]}! Vamos ao cardápio.");
        Thread.Sleep(1500); // pausa
        Console.Clear();

        // Loop do menu principal
        bool continuar = true;
        while (continuar)
        {
            ExibirMenuPrincipal();
            switch (Console.ReadLine()?.Trim())
            {
                case "1": EscolherDoCardapio(pedido, Cardapio.Lanches.Cast<Produto>().ToList(),    "LANCHES");    break;
                case "2": EscolherDoCardapio(pedido, Cardapio.Bebidas.Cast<Produto>().ToList(),    "BEBIDAS");    break;
                case "3": EscolherDoCardapio(pedido, Cardapio.Sobremesas.Cast<Produto>().ToList(), "SOBREMESAS"); break;
                case "4": EscolherComboPromocional(pedido); break;
                case "5": MontarComboPersonalizado(pedido); break;
                case "6": ListarERemoverItens(pedido); break;
                case "0": continuar = false; break;
                default:  Console.WriteLine("\nOpção inválida."); break;
            }
        }

        // Finaliza
        if (pedido.Itens.Count == 0)
        {
            Console.WriteLine("\nPedido cancelado. Até a próxima!");
        }
        else
        {
            pedido.ExibirResumo();
        }

        Console.WriteLine("\nPressione qualquer tecla para sair.");
        Console.ReadKey();
    }

    // API VIACEP
    static async Task<Cliente> CadastrarClienteAsync()
    {
        Console.WriteLine("Antes de começar, precisamos de alguns dados:");
        Console.WriteLine();

        string nome;
        do
        {
            Console.Write("  Seu nome: ");
            nome = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(nome))
                Console.WriteLine("  Por favor, informe seu nome.");
        } while (string.IsNullOrWhiteSpace(nome));

        Console.Write("  Telefone: ");
        string telefone = Console.ReadLine()?.Trim() ?? "";

        Console.WriteLine();
        Console.WriteLine("Para entrega, informe seu endereço:");

        var via = new ViaCepService();
        Endereco endereco = null;
        while (endereco == null)
        {
            Console.Write("  CEP: ");
            string cep = Console.ReadLine()?.Trim() ?? "";
            try
            {
                Console.WriteLine("  Buscando seu endereço...");
                endereco = await via.ConsultarCepAsync(cep);
                Console.WriteLine($"  {endereco.Logradouro} - {endereco.Bairro}, {endereco.Cidade}/{endereco.Uf}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {ex.Message} Tente novamente.");
            }
        }

        Console.Write("  Número: ");
        endereco.Numero = Console.ReadLine()?.Trim() ?? "S/N";

        Console.Write("  Complemento (ENTER para pular): ");
        var compl = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(compl))
            endereco.Complemento = compl;

        return new Cliente { Nome = nome, Telefone = telefone, Endereco = endereco };
    }

    // MENU
    static void ExibirMenuPrincipal()
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("  O que vai pedir?");
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("  [1] Lanches");
        Console.WriteLine("  [2] Bebidas");
        Console.WriteLine("  [3] Sobremesas");
        Console.WriteLine("  [4] Combos da casa");
        Console.WriteLine("  [5] Monte seu combo (15% de desconto)");
        Console.WriteLine("  [6] Ver pedido");
        Console.WriteLine("  [0] Fechar pedido");
        Console.Write("  > ");
    }

    // ESCOLHER DO CARDÁPIO 
    static void EscolherDoCardapio(Pedido pedido, List<Produto> opcoes, string titulo)
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine($"  {titulo}");
        Console.WriteLine("--------------------------------------------------------------");

        var escolhido = EscolherItem(opcoes);
        if (escolhido == null)
        {
            Console.WriteLine("  Voltando ao menu.");
            return;
        }
        pedido.AdicionarProduto(escolhido);
        Console.WriteLine($"\n  {escolhido.Nome} adicionado ao pedido.");
    }


    /// lista as opções em duas linhas (nome+preço / descrição)
    /// e devolve a escolhida (ou null). Funciona para qualquer Produto graças ao polimorfismo do método Detalhe(), VITORIA, presta atenção no polimofismo, tenta copiar ele e entender.
    static Produto EscolherItem(List<Produto> opcoes)
    {
        Console.WriteLine();
        for (int i = 0; i < opcoes.Count; i++)
        {
            var item = opcoes[i];
            string esquerda = $"  [{i + 1}]  {item.Nome}";
            string direita = Formatador.Preco(item.Preco);
            Console.WriteLine(Formatador.LinhaComPontos(esquerda, direita));
            Console.WriteLine($"       {item.Detalhe()}");
            Console.WriteLine();
        }
        Console.WriteLine("  [0]  Voltar");
        Console.Write("\n  > ");

        if (!int.TryParse(Console.ReadLine(), out int n) || n < 1 || n > opcoes.Count)
            return null;
        return opcoes[n - 1];
    }

    // COMBOS PRONTOS
    static void EscolherComboPromocional(Pedido pedido)
    {
        // Cada combo tem nome, preço promocional e os itens que o compõem, tipo montar um no mc ou bk
        var combos = new List<(string nome, decimal preco, Produto[] itens)>
        {
            ("Combo Clássico", 35.00m, new Produto[] { Cardapio.Lanches[0], Cardapio.Bebidas[0], Cardapio.Sobremesas[0] }),
            ("Combo Família",  55.00m, new Produto[] { Cardapio.Lanches[3], Cardapio.Bebidas[1], Cardapio.Sobremesas[1] }),
            ("Combo Light",    32.00m, new Produto[] { Cardapio.Lanches[1], Cardapio.Bebidas[3], Cardapio.Sobremesas[2] })
        };

        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("  COMBOS DA CASA");
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine();

        for (int i = 0; i < combos.Count; i++)
        {
            var c = combos[i];
            decimal soma = c.itens.Sum(x => x.Preco);
            decimal economia = soma - c.preco;

            Console.WriteLine(Formatador.LinhaComPontos($"  [{i + 1}]  {c.nome}", Formatador.Preco(c.preco)));
            Console.WriteLine($"       {string.Join(" + ", c.itens.Select(x => x.Nome))}");
            if (economia > 0)
                Console.WriteLine($"       Economia de {Formatador.Preco(economia)}");
            Console.WriteLine();
        }
        Console.WriteLine("  [0]  Voltar");
        Console.Write("\n  > ");

        if (!int.TryParse(Console.ReadLine(), out int n) || n < 1 || n > combos.Count)
        {
            Console.WriteLine("  Voltando ao menu.");
            return;
        }

        var sel = combos[n - 1];
        var combo = new Combo(sel.nome, sel.preco);
        foreach (var it in sel.itens) combo.AdicionarItem(it);
        pedido.AdicionarProduto(combo);
        Console.WriteLine($"\n  {sel.nome} adicionado ao pedido.");
    }

    // COMBO PERSONALIZADO
    static void MontarComboPersonalizado(Pedido pedido)
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("  MONTE SEU COMBO");
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("  Escolha 1 lanche, 1 bebida e 1 sobremesa.");
        Console.WriteLine("  Você leva 15% de desconto sobre o valor dos três itens.");

        Console.WriteLine("\n  Passo 1 de 3 - Lanche");
        var lanche = EscolherItem(Cardapio.Lanches.Cast<Produto>().ToList()) as Lanche;
        if (lanche == null) { Console.WriteLine("  Combo cancelado."); return; }


        Console.Clear();
        Console.WriteLine("\n  Passo 2 de 3 - Bebida");
        var bebida = EscolherItem(Cardapio.Bebidas.Cast<Produto>().ToList()) as Bebida;
        if (bebida == null) { Console.WriteLine("  Combo cancelado."); return; }

        Console.Clear();
        Console.WriteLine("\n  Passo 3 de 3 - Sobremesa");
        var sobremesa = EscolherItem(Cardapio.Sobremesas.Cast<Produto>().ToList()) as Sobremesa;
        if (sobremesa == null) { Console.WriteLine("  Combo cancelado."); return; }

        decimal soma = lanche.Preco + bebida.Preco + sobremesa.Preco;
        decimal precoCombo = Math.Round(soma * 0.85m, 2); // 15% de desconto
        decimal economia = soma - precoCombo;

        var combo = new Combo("Combo Personalizado", precoCombo);
        combo.AdicionarItem(lanche);
        combo.AdicionarItem(bebida);
        combo.AdicionarItem(sobremesa);
        pedido.AdicionarProduto(combo);

        Console.Clear();
        Console.WriteLine($"\n  Combo montado por {Formatador.Preco(precoCombo)}.");
        Console.WriteLine($"  Você economizou {Formatador.Preco(economia)}.");
        Thread.Sleep(2000);
    }

    // VER OU REMOVER ITENS
    static void ListarERemoverItens(Pedido pedido)
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine("  SEU PEDIDO ATÉ AGORA");
        Console.WriteLine("--------------------------------------------------------------");

        if (pedido.Itens.Count == 0)
        {
            Console.WriteLine("  Nenhum item adicionado ainda.");
            return;
        }

        Console.WriteLine();
        for (int i = 0; i < pedido.Itens.Count; i++)
        {
            var item = pedido.Itens[i];
            Console.WriteLine(Formatador.LinhaComPontos($"  [{i + 1}]  {item.Nome}", Formatador.Preco(item.Preco)));
            Console.WriteLine($"       {item.Detalhe()}");
            Console.WriteLine();
        }
        Console.WriteLine($"  Subtotal até aqui: {Formatador.Preco(pedido.CalcularSubtotal())}");

        Console.Write("\n  Digite o número do item para REMOVER (0 para voltar): ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n < 1) return;
        if (pedido.RemoverProduto(n - 1))
            Console.WriteLine("  Item removido.");
        else
            Console.WriteLine("  Número inválido.");
    }
}