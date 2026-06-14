import React, { useEffect, useMemo, useState } from "react";
import { createRoot } from "react-dom/client";
import "./styles.css";

const tabs = ["Stock", "Movement", "Master", "Reports"];

const emptyMovement = {
  type: "import",
  referenceNo: "",
  fromWarehouseId: "",
  toWarehouseId: "",
  productId: "",
  quantity: 1,
  unitCost: "",
  note: ""
};

function App() {
  const [apiUrl, setApiUrl] = useState("http://localhost:5000");
  const [token, setToken] = useState(localStorage.getItem("eims-token") || "");
  const [auth, setAuth] = useState(null);
  const [tab, setTab] = useState("Stock");
  const [message, setMessage] = useState("");
  const [stock, setStock] = useState({ items: [], total: 0 });
  const [products, setProducts] = useState([]);
  const [warehouses, setWarehouses] = useState([]);
  const [categories, setCategories] = useState([]);
  const [valuation, setValuation] = useState([]);
  const [history, setHistory] = useState([]);
  const [movement, setMovement] = useState(emptyMovement);

  const authHeaders = useMemo(
    () => ({
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    }),
    [token]
  );

  async function request(path, options = {}) {
    const response = await fetch(`${apiUrl.replace(/\/$/, "")}${path}`, {
      ...options,
      headers: { ...authHeaders, ...(options.headers || {}) }
    });

    if (!response.ok) {
      const problem = await response.json().catch(() => ({}));
      throw new Error(problem.detail || response.statusText);
    }

    return response.status === 204 ? null : response.json();
  }

  async function refresh() {
    if (!token) return;
    const [nextCategories, nextProducts, nextWarehouses, nextStock] = await Promise.all([
      request("/api/master-data/categories"),
      request("/api/master-data/products"),
      request("/api/master-data/warehouses"),
      request("/api/inventory/stock?page=1&pageSize=50")
    ]);

    setCategories(nextCategories);
    setProducts(nextProducts);
    setWarehouses(nextWarehouses);
    setStock(nextStock);

    const [nextValuation, nextHistory] = await Promise.all([
      request("/api/reports/stock-valuation").catch(() => []),
      request("/api/reports/movement-history").catch(() => [])
    ]);
    setValuation(nextValuation);
    setHistory(nextHistory);

    setMovement((current) => ({
      ...current,
      productId: current.productId || nextProducts[0]?.id || "",
      fromWarehouseId: current.fromWarehouseId || nextWarehouses[0]?.id || "",
      toWarehouseId: current.toWarehouseId || nextWarehouses[0]?.id || ""
    }));
  }

  async function handleLogin(event) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const data = await request("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        userName: form.get("userName"),
        password: form.get("password")
      })
    });
    localStorage.setItem("eims-token", data.accessToken);
    setToken(data.accessToken);
    setAuth(data);
    setMessage(`Logged in as ${data.userName}`);
  }

  async function createCategory(event) {
    event.preventDefault();
    const name = new FormData(event.currentTarget).get("name");
    await request("/api/master-data/categories", {
      method: "POST",
      body: JSON.stringify({ name, parentId: null })
    });
    event.currentTarget.reset();
    setMessage("Category created");
    await refresh();
  }

  async function createWarehouse(event) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await request("/api/master-data/warehouses", {
      method: "POST",
      body: JSON.stringify({ code: form.get("code"), name: form.get("name"), parentId: null })
    });
    event.currentTarget.reset();
    setMessage("Warehouse created");
    await refresh();
  }

  async function createProduct(event) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await request("/api/master-data/products", {
      method: "POST",
      body: JSON.stringify({
        sku: form.get("sku"),
        name: form.get("name"),
        categoryId: form.get("categoryId"),
        unitCost: Number(form.get("unitCost")),
        minStockLevel: Number(form.get("minStockLevel")),
        isActive: true
      })
    });
    event.currentTarget.reset();
    setMessage("Product created");
    await refresh();
  }

  async function postMovement(event) {
    event.preventDefault();
    const line = {
      productId: movement.productId,
      quantity: Number(movement.quantity),
      unitCost: movement.unitCost === "" ? null : Number(movement.unitCost)
    };
    const body = {
      referenceNo: movement.referenceNo || `${movement.type.toUpperCase()}-${Date.now()}`,
      note: movement.note,
      lines: [line]
    };

    if (movement.type === "transfer") {
      body.fromWarehouseId = movement.fromWarehouseId;
      body.toWarehouseId = movement.toWarehouseId;
    } else {
      body.warehouseId = movement.type === "import" ? movement.toWarehouseId : movement.fromWarehouseId;
    }

    await request(`/api/inventory/${movement.type}`, {
      method: "POST",
      body: JSON.stringify(body)
    });
    setMovement((current) => ({ ...emptyMovement, productId: current.productId, fromWarehouseId: current.fromWarehouseId, toWarehouseId: current.toWarehouseId }));
    setMessage("Movement posted");
    await refresh();
  }

  async function reconcile(event) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await request("/api/inventory/reconcile", {
      method: "POST",
      body: JSON.stringify({
        warehouseId: form.get("warehouseId"),
        productId: form.get("productId"),
        actualQuantity: Number(form.get("actualQuantity")),
        reason: form.get("reason")
      })
    });
    event.currentTarget.reset();
    setMessage("Adjustment applied");
    await refresh();
  }

  useEffect(() => {
    if (!token) return;
    refresh().catch((error) => setMessage(error.message));
  }, [token]);

  return (
    <main className="page">
      <header className="topbar">
        <div>
          <p className="eyebrow">EIMS</p>
          <h1>Inventory</h1>
        </div>
        <button onClick={() => refresh().catch((error) => setMessage(error.message))}>Refresh</button>
      </header>

      <section className="login">
        <label>
          API
          <input value={apiUrl} onChange={(event) => setApiUrl(event.target.value)} />
        </label>
        <form onSubmit={(event) => handleLogin(event).catch((error) => setMessage(error.message))}>
          <input name="userName" defaultValue="admin" aria-label="Username" />
          <input name="password" type="password" aria-label="Password" placeholder="Password" />
          <button>Login</button>
        </form>
        <span>{auth ? `${auth.userName} / ${auth.role}` : token ? "Token loaded" : "Not connected"}</span>
      </section>

      <nav className="tabs">
        {tabs.map((item) => (
          <button key={item} className={tab === item ? "active" : ""} onClick={() => setTab(item)}>
            {item}
          </button>
        ))}
      </nav>

      {message && <p className="message">{message}</p>}

      {tab === "Stock" && <StockView stock={stock} />}
      {tab === "Movement" && (
        <MovementView
          movement={movement}
          setMovement={setMovement}
          products={products}
          warehouses={warehouses}
          postMovement={postMovement}
          reconcile={reconcile}
        />
      )}
      {tab === "Master" && (
        <MasterView
          categories={categories}
          createCategory={createCategory}
          createWarehouse={createWarehouse}
          createProduct={createProduct}
        />
      )}
      {tab === "Reports" && <ReportsView valuation={valuation} history={history} />}
    </main>
  );
}

function StockView({ stock }) {
  return (
    <section className="panel">
      <div className="section-title">
        <h2>Current Stock</h2>
        <span>{stock.total} rows</span>
      </div>
      <Table
        headers={["SKU", "Product", "Warehouse", "Qty", "Min"]}
        rows={stock.items.map((item) => [item.sku, item.productName, item.warehouseCode, item.quantity, item.minStockLevel])}
      />
    </section>
  );
}

function MovementView({ movement, setMovement, products, warehouses, postMovement, reconcile }) {
  const update = (key) => (event) => setMovement((current) => ({ ...current, [key]: event.target.value }));

  return (
    <section className="grid">
      <form className="panel form" onSubmit={(event) => postMovement(event).catch(console.error)}>
        <h2>Movement</h2>
        <select value={movement.type} onChange={update("type")}>
          <option value="import">Import</option>
          <option value="export">Export</option>
          <option value="transfer">Transfer</option>
        </select>
        <input placeholder="Reference" value={movement.referenceNo} onChange={update("referenceNo")} />
        <Select value={movement.fromWarehouseId} onChange={update("fromWarehouseId")} items={warehouses} label={(item) => item.code} />
        <Select value={movement.toWarehouseId} onChange={update("toWarehouseId")} items={warehouses} label={(item) => item.code} />
        <Select value={movement.productId} onChange={update("productId")} items={products} label={(item) => `${item.sku} - ${item.name}`} />
        <input type="number" min="0.001" step="0.001" value={movement.quantity} onChange={update("quantity")} />
        <input type="number" min="0" step="0.01" placeholder="Unit cost" value={movement.unitCost} onChange={update("unitCost")} />
        <input placeholder="Note" value={movement.note} onChange={update("note")} />
        <button>Post</button>
      </form>

      <form className="panel form" onSubmit={(event) => reconcile(event).catch(console.error)}>
        <h2>Reconcile</h2>
        <select name="warehouseId" required>{warehouses.map((item) => <option key={item.id} value={item.id}>{item.code}</option>)}</select>
        <select name="productId" required>{products.map((item) => <option key={item.id} value={item.id}>{item.sku}</option>)}</select>
        <input name="actualQuantity" type="number" min="0" step="0.001" placeholder="Actual quantity" required />
        <input name="reason" placeholder="Reason" />
        <button>Adjust</button>
      </form>
    </section>
  );
}

function MasterView({ categories, createCategory, createWarehouse, createProduct }) {
  return (
    <section className="grid three">
      <form className="panel form" onSubmit={(event) => createCategory(event).catch(console.error)}>
        <h2>Category</h2>
        <input name="name" placeholder="Name" required />
        <button>Create</button>
      </form>
      <form className="panel form" onSubmit={(event) => createWarehouse(event).catch(console.error)}>
        <h2>Warehouse</h2>
        <input name="code" placeholder="Code" required />
        <input name="name" placeholder="Name" required />
        <button>Create</button>
      </form>
      <form className="panel form" onSubmit={(event) => createProduct(event).catch(console.error)}>
        <h2>Product</h2>
        <input name="sku" placeholder="SKU" required />
        <input name="name" placeholder="Name" required />
        <select name="categoryId" required>{categories.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select>
        <input name="unitCost" type="number" min="0" step="0.01" placeholder="Unit cost" required />
        <input name="minStockLevel" type="number" min="0" step="0.001" placeholder="Min stock" required />
        <button>Create</button>
      </form>
    </section>
  );
}

function ReportsView({ valuation, history }) {
  return (
    <section className="grid">
      <div className="panel">
        <h2>Valuation</h2>
        <Table headers={["SKU", "Warehouse", "Qty", "Value"]} rows={valuation.map((item) => [item.sku, item.warehouseCode, item.quantity, Number(item.totalValue).toFixed(2)])} />
      </div>
      <div className="panel">
        <h2>History</h2>
        <Table headers={["Ref", "Type", "SKU", "Qty"]} rows={history.map((item) => [item.referenceNo, item.type, item.sku, item.quantity])} />
      </div>
    </section>
  );
}

function Select({ items, label, ...props }) {
  return (
    <select {...props}>
      {items.map((item) => (
        <option key={item.id} value={item.id}>
          {label(item)}
        </option>
      ))}
    </select>
  );
}

function Table({ headers, rows }) {
  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>{headers.map((header) => <th key={header}>{header}</th>)}</tr>
        </thead>
        <tbody>
          {rows.map((row, index) => (
            <tr key={index}>{row.map((cell, cellIndex) => <td key={cellIndex}>{cell}</td>)}</tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

createRoot(document.getElementById("root")).render(<App />);
