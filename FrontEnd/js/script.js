document.getElementById("orderForm").addEventListener("submit", function(event) {
    event.preventDefault(); 

    const data = {
        symbol: document.getElementById("symbol").value,
        side: document.getElementById("side").value,
        quantity: parseInt(document.getElementById("quantity").value),
        price: parseFloat(document.getElementById("price").value)
    };

    fetch("http://localhost:5086/api/orders", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    })
    .then(response => {
        return response.json().then(data => {
            console.log("Mensagem recebida:", data.message);

            if (data.message && data.message.includes("Excedido limite de exposições para símbolo")) {
                throw new Error(data.message); 
            }

            if (!response.ok) {
                if (data.errors && data.errors.Quantity && data.errors.Quantity.length > 0) {
                    const quantityError = data.errors.Quantity[0];
                    throw new Error(quantityError);
                }
                throw new Error("Erro desconhecido ao tentar criar a ordem.");
            }

            return data; 
        });
    })
    .then(data => {
        if (data.message) {
            alert("Order Generator: " + data.message); 
        }
    })
    .catch(error => {
        console.error('Erro ao criar a ordem:', error);
        
        alert("Erro: " + error.message || "Erro desconhecido.");
    });
});