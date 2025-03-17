// Adiciona um ouvinte de evento ao formulário
document.getElementById("orderForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Previne o comportamento padrão de envio do formulário

    // Obtém os dados do formulário
    const data = {
        symbol: document.getElementById("symbol").value,
        side: document.getElementById("side").value,
        quantity: parseInt(document.getElementById("quantity").value),
        price: parseFloat(document.getElementById("price").value)
    };

    // Faz a requisição POST para a API
    fetch("http://localhost:5086/api/orders", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    })
    .then(response => {
        if (!response.ok) {
            // Se a resposta não for bem-sucedida, verifica se há erro no corpo da resposta
            return response.json().then(errorData => {
                // Pega o primeiro erro relacionado ao campo "Quantity"
                if (errorData.errors && errorData.errors.Quantity && errorData.errors.Quantity.length > 0) {
                    const quantityError = errorData.errors.Quantity[0]; // Pega o primeiro erro
                    throw new Error(quantityError);
                }
                throw new Error("Erro desconhecido ao tentar criar a ordem.");
            });
        }
        return response.json();
    })
    .then(data => {
        // Se a resposta for bem-sucedida, exibe a mensagem de sucesso
        if (data.message) {
            alert(data.message); // Exibe mensagem de sucesso
        }
    })
    .catch(error => {
        // Exibe a mensagem de erro caso a API retorne um erro
        console.error('Erro ao criar a ordem:', error);
        
        // Aqui mostramos apenas a mensagem de erro
        alert("Erro: " + error.message || "Erro desconhecido.");
    });
});
