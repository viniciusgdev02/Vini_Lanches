using System;
using System.Collections.Generic;
using System.Linq;

namespace Lanchonete.Models
{
  
    /// Dados do cliente
    public class Cliente
    {
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public Endereco Endereco { get; set; }
    }

    /// Endereço completo, ele é preenchido via consulta ao ViaCEP

    public class Endereco
    {
        public string Cep { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Uf { get; set; }

        public override string ToString()
        {
            var compl = string.IsNullOrWhiteSpace(Complemento) ? "" : $" - {Complemento}";
            return $"{Logradouro}, {Numero}{compl} - {Bairro}, {Cidade}/{Uf} - CEP: {Cep}";
        }
    }


//ABSTRATA:  UMA CLASSE ABSTRATA NÃO PODE SER INSTANCIADA, ELA NÃO FUNCIONA PARA NADA ALEM DE SER HERDADA. ELA SERVE PARA DEFINIRUM MOLDE, UMA BASE COM PROPRIEDADES E MÉTODOS QUE AS CLASSES FILHAS VÃO IMPLEMENTAR
    
// ABSTRAÇÃO: classe abstrata base, não pode ser instanciada, apenas serve de molde para os produtos reais.
    
 //ENCAPSULAMENTO: campos privados com validação nas propriedades.
    
// POLIMORFISMO: Detalhe() é abstrato e cada filha define sua própria forma de descrever o atributo.


// Se tiver dúvida sobre o "=>":
// Ele é uma forma mais curta de escrever métodos ou propriedades simples em C#.

// Exemplo:
// "get => _nome;"

// É a mesma coisa que escrever:
// "get { return _nome; }"

// Ou seja, o "=>" serve para retornar um valor de forma mais simples e limpa.
// Ele é muito usado quando o código possui apenas uma linha.

    public abstract class Produto
    {
        private string _nome;
        private decimal _preco;

        public string Nome
        {
            get => _nome;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Nome do produto é obrigatório.");
                _nome = value;
            }
        }

        public decimal Preco
        {
            get => _preco;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Preço não pode ser negativo.");
                _preco = value;
            }
        }

        protected Produto() { }
        protected Produto(string nome, decimal preco)
        {
            Nome = nome;
            Preco = preco;
        }

        /// Descrição curta do item, só a parte específica da subclasse, sem nome nem preço. Ex: para Lanche são os ingredientes, para Bebida é o tamanho em ml
        
        public abstract string Detalhe();

        /// Linha completa formatada (nome + preço + detalhe).
        public virtual string ExibirDetalhes() => $"{Nome} - R$ {Preco:F2} - {Detalhe()}";
    }

    //HERANÇA: Lanche É UM Produto e adiciona seus ingredientes.
    
    public class Lanche : Produto
    {
        public List<string> Ingredientes { get; set; } = new List<string>();

        public Lanche() { }
        public Lanche(string nome, decimal preco, List<string> ingredientes) : base(nome, preco)
        {
            Ingredientes = ingredientes ?? new List<string>();
        }

        public override string Detalhe() => string.Join(", ", Ingredientes);
    }

    public class Bebida : Produto
    {
        public int TamanhoML { get; set; }

        public Bebida() { }
        public Bebida(string nome, decimal preco, int tamanhoML) : base(nome, preco)
        {
            TamanhoML = tamanhoML;
        }

        public override string Detalhe() => $"{TamanhoML} ml";
    }

    public class Sobremesa : Produto
    {
        // Descrição livre — ex: "tradicional com calda de caramelo".
        public string Descricao { get; set; }

        public Sobremesa() { }
        public Sobremesa(string nome, decimal preco, string descricao) : base(nome, preco)
        {
            Descricao = descricao;
        }

        public override string Detalhe() => Descricao;
    }

    //  A categoria Combo foi absorvida sem quebrar nada do sistema existente.
    //  Herda de Produto, então é tratada como qualquer outro item do pedido
    //  Agrega outros Produtos por composição
    // Tem preço promocional próprio (não é a soma dos itens)
    //POLIMORFISMO: o Pedido nem precisa saber que é um Combo, chama Detalhe() e o C# resolve qual implementação executar.
    public class Combo : Produto
    {
        public List<Produto> ItensDoCombo { get; set; } = new List<Produto>();

        public Combo() { }
        public Combo(string nome, decimal precoPromocional) : base(nome, precoPromocional) { }

        public void AdicionarItem(Produto item) => ItensDoCombo.Add(item);

        public override string Detalhe() =>
            string.Join(" + ", ItensDoCombo.Select(i => i.Nome));
    }
}