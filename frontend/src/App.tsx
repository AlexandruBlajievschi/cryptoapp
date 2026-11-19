import './App.css'
import { Routes, Route, Link } from 'react-router'
import Account from './pages/Account'
import Dashboard from './pages/Dashboard'
import Learn from './pages/Learn'
import Portfolio from './pages/Portfolio'
import Premium from './pages/Premium'
import Watchlist from './pages/Watchlist'
import NotFound from './pages/NotFound'
import Home from './pages/Home'


function App() {
  return (
    <>
      <nav>
        <Link to="/">Home</Link>
        <Link to="/account">Account</Link>
        <Link to="/dashboard">Dashboard</Link>
        <Link to="/learn">Learn</Link>
        <Link to="/portfolio">Portfolio</Link>
        <Link to="/premium">Premium</Link>
        <Link to="/watchlist">Watchlist</Link>
      </nav>

      <Routes>
        <Route path='/' element={<Home />}></Route>
        <Route path='/account' element={<Account />} />
        <Route path='/dashboard' element={<Dashboard />} />
        <Route path='/learn' element={<Learn />} />
        <Route path='/portfolio' element={<Portfolio />} />
        <Route path='/premium' element={<Premium />} />
        <Route path='/watchlist' element={<Watchlist />} />
        <Route path="*" element={<NotFound />} />

      </Routes>
    </>

  )
}

export default App
