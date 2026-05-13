🍔 Vini Lanches - Sistema de Gestão de Pedidos
Um sistema de console robusto para gerenciamento de pedidos de uma lanchonete, desenvolvido em C#. O projeto simula o fluxo completo desde o cadastro do cliente (com busca automática de endereço) até a finalização do pedido com cálculos de descontos e taxas.

🚀 Funcionalidades
Cadastro de Cliente via CEP: Integração em tempo real com a API ViaCEP para preenchimento automático de endereço.

Cardápio Dinâmico: Categorias para Lanches, Bebidas e Sobremesas.

Sistema de Combos:

Combos da Casa: Opções pré-definidas com preços promocionais.

Combo Personalizado: O usuário escolhe os itens e recebe 15% de desconto automaticamente.

Gestão de Carrinho: Permite listar itens adicionados e remover produtos antes de fechar a conta.

Cálculo de Fechamento:

Desconto automático de 10% para pedidos acima de R$ 50,00.

Aplicação de taxa de entrega fixa.

Estimativa de tempo de entrega aleatória.

🛠️ Conceitos de POO Aplicados
Este projeto foi desenhado para demonstrar o uso prático dos quatro pilares da POO:

Abstração: Uso da classe abstrata Produto, que serve de molde para todos os itens comercializáveis, garantindo que nenhum "produto genérico" seja instanciado.

Herança: As classes Lanche, Bebida, Sobremesa e Combo herdam propriedades e comportamentos de Produto, promovendo o reuso de código.

Encapsulamento: Proteção de dados sensíveis (como preços e listas de itens) através de propriedades com validação (get/set) e o uso de IReadOnlyList.

Polimorfismo: O método Detalhe() é sobrescrito em cada subclasse. Isso permite que o sistema trate diferentes objetos de forma uniforme na lista de pedidos, chamando a implementação específica de cada um em tempo de execução.

📂 Estrutura do Projeto
Program.cs: Orquestrador do fluxo principal, menus e interação com o usuário.

Models.cs: Contém as entidades do sistema (Cliente, Endereco, Produto e suas derivações).

Services.cs:

ViaCepService: Gerencia a comunicação assíncrona com a API externa.

Pedido: Responsável pela lógica de negócio, cálculos e exibição do resumo.

Formatador: Utilitário para interface visual e formatação de moeda.

🔧 Como Executar
Certifique-se de ter o SDK do .NET 6.0 (ou superior) instalado.

Clone o repositório:

Bash git clone https://github.com/seu-usuario/Vini_Lanches.git

Bash cd Vini_lanches
Execute a aplicação:

Bash
dotnet run
🌐 Tecnologias Utilizadas
Linguagem: C#

Framework: .NET

API Externa: ViaCEP

Bibliotecas: System.Net.Http (Requisições), System.Text.Json (Deserialização).

⭐ Este projeto faz parte dos meus estudos de Programação Orientada a Objetos e Desenvolvimento Backend.
