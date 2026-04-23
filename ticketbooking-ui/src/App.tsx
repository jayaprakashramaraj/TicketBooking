import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Booking from './pages/Booking';
import Admin from './pages/Admin';
import MyBookings from './pages/MyBookings';
import BookingResult from './pages/BookingResult';
import { User, LogOut, Ticket, ClipboardList } from 'lucide-react';
import { useState, useEffect } from 'react';
import { Navbar, Nav, Container, Button, Dropdown } from 'react-bootstrap';

function App() {
  const [user, setUser] = useState<{ fullName: string; role: string } | null>(null);

  useEffect(() => {
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      try {
        setUser(JSON.parse(savedUser));
      } catch (e) {
        console.error('Failed to parse user from localStorage', e);
        localStorage.removeItem('user');
      }
    }
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('user');
    setUser(null);
    window.location.href = '/';
  };

  return (
    <Router>
      <div className="d-flex flex-column min-vh-100">
        <Navbar expand="lg" variant="dark" className="sticky-top shadow-sm py-3">
          <Container>
            <Navbar.Brand as={Link as any} to="/" className="d-flex align-items-center gap-2">
              <Ticket className="text-primary" size={32} />
              <span className="fw-bold fs-3 text-white">TicketSwift</span>
            </Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Navbar.Collapse id="basic-navbar-nav">
              <Nav className="ms-auto align-items-center gap-3">
                <Nav.Link as={Link as any} to="/" className="text-light">Home</Nav.Link>
                {user ? (
                  <>
                    {user.role === 'Admin' && <Nav.Link as={Link as any} to="/admin" className="text-light">Admin</Nav.Link>}
                    <Dropdown align="end">
                      <Dropdown.Toggle variant="link" className="text-light text-decoration-none d-flex align-items-center gap-2 border-0 p-0">
                        <User size={20} />
                        <span>{user.fullName}</span>
                      </Dropdown.Toggle>
                      <Dropdown.Menu variant="dark">
                        <Dropdown.Item as={Link as any} to="/my-bookings">
                          <ClipboardList size={16} className="me-2" /> My Bookings
                        </Dropdown.Item>
                        <Dropdown.Divider className="bg-secondary opacity-25" />
                        <Dropdown.Item onClick={handleLogout} className="text-danger">
                          <LogOut size={16} className="me-2" /> Logout
                        </Dropdown.Item>
                      </Dropdown.Menu>
                    </Dropdown>
                  </>
                ) : (
                  <>
                    <Nav.Link as={Link as any} to="/login" className="text-light">Login</Nav.Link>
                    <Button as={Link as any} to="/register" variant="primary" className="rounded-pill px-4">Register</Button>
                  </>
                )}
              </Nav>
            </Navbar.Collapse>
          </Container>
        </Navbar>

        <main className="py-5">
          <Container>
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/book/:showId" element={<Booking />} />
              <Route path="/booking-result" element={<BookingResult />} />
              <Route path="/admin" element={<Admin />} />
              <Route path="/my-bookings" element={<MyBookings />} />
            </Routes>
          </Container>
        </main>

        <footer className="mt-auto py-4 bg-dark border-top border-secondary">
          <Container className="text-center text-secondary">
            <p className="mb-0">© 2026 TicketSwift. All rights reserved.</p>
          </Container>
        </footer>
      </div>
    </Router>
  );
}

export default App;
