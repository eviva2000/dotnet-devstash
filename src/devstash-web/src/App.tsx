import './App.css'

function App() {
  return (
    <main>
      <p className="eyebrow">DevStash</p>
      <h1>.NET + React foundation</h1>
      <p className="summary">
        The frontend is running. Next, we will connect it to the ASP.NET Core API.
      </p>

      <dl>
        <div>
          <dt>Backend</dt>
          <dd>ASP.NET Core 10</dd>
        </div>
        <div>
          <dt>Frontend</dt>
          <dd>React + TypeScript + Vite</dd>
        </div>
        <div>
          <dt>Status</dt>
          <dd className="ready">Foundation ready</dd>
        </div>
      </dl>
    </main>
  )
}

export default App
