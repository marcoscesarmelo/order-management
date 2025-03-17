using System.ComponentModel.DataAnnotations;

namespace OrderGenerator.Models
{
    public class Order
    {
        [Required(ErrorMessage = "Símbolo é obrigatório")]
        [RegularExpression(@"^(PETR4|VALE3|VIIA4)$", ErrorMessage = "Símbolo inválido. Escolha entre PETR4, VALE3 ou VIIA4.")]
        public string? Symbol { get; set; }

        [Required(ErrorMessage = "Lado é obrigatório")]
        [RegularExpression(@"^(Compra|Venda)$", ErrorMessage = "Lado inválido. Escolha entre Compra ou Venda.")]
        public string? Side { get; set; }

        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(1, 99999, ErrorMessage = "Quantidade deve ser um valor positivo menor que 100.000.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Preço é obrigatório")]
        [Range(0.01, 999.99, ErrorMessage = "Preço deve ser um valor positivo múltiplo de 0.01 e menor que 1.000.")]
        public decimal Price { get; set; }
    }
}
