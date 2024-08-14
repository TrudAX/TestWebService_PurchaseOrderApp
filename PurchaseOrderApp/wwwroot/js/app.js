// wwwroot/js/app.js
async function loadPurchaseOrders() {
    try {
        const response = await axios.get('/api/purchaseorder');
        const purchaseOrders = response.data;
        const purchaseOrdersDiv = document.getElementById('purchaseOrders');
        purchaseOrdersDiv.innerHTML = '';
        purchaseOrders.forEach(po => {
            const poDiv = document.createElement('div');
            poDiv.innerHTML = `
                <h3>Purchase Order: ${po.purchId}</h3>
                <p>Order Account: ${po.orderAccount}</p>
                <h4>Lines:</h4>
                <ul>
                    ${po.lines.map(line => `
                        <li>Item: ${line.itemId}, Quantity: ${line.quantity}, Amount: ${line.lineAmount}</li>
                    `).join('')}
                </ul>
                <button onclick="deletePurchaseOrder(${po.id})">Delete</button>
            `;
            purchaseOrdersDiv.appendChild(poDiv);
        });
    } catch (error) {
        console.error('Error loading purchase orders:', error);
    }
}

async function createPurchaseOrder(event) {
    event.preventDefault();
    const purchId = document.getElementById('purchId').value;
    const orderAccount = document.getElementById('orderAccount').value;
    try {
        await axios.post('/api/purchaseorder', { purchId, orderAccount, lines: [] });
        loadPurchaseOrders();
        document.getElementById('createForm').reset();
    } catch (error) {
        console.error('Error creating purchase order:', error);
    }
}

async function deletePurchaseOrder(id) {
    try {
        await axios.delete(`/api/purchaseorder/${id}`);
        loadPurchaseOrders();
    } catch (error) {
        console.error('Error deleting purchase order:', error);
    }
}

document.getElementById('createForm').addEventListener('submit', createPurchaseOrder);

// Load purchase orders on page load
loadPurchaseOrders();