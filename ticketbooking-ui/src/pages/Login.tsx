import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import API_BASE_URL from '../config';
import { LogIn, Mail, Lock, Info } from 'lucide-react';
import { Container, Row, Col, Card, Form, Button, Alert, InputGroup, Spinner } from 'react-bootstrap';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await fetch(`${API_BASE_URL.IDENTITY}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      if (response.ok) {
        const user = await response.json();
        localStorage.setItem('user', JSON.stringify(user));
        navigate('/');
        window.location.reload();
      } else {
        setError('Invalid credentials. Please try again.');
      }
    } catch (err) {
      setError('Connection failed. Please check your backend.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="py-5">
      <Row className="justify-content-center">
        <Col md={6} lg={5} xl={4}>
          <Card className="shadow-lg border-0 bg-dark text-white rounded-3 overflow-hidden">
            <div className="bg-primary p-4 text-center">
              <div className="bg-white bg-opacity-25 rounded-circle d-inline-flex p-3 mb-3">
                <LogIn size={32} className="text-white" />
              </div>
              <h2 className="fw-bold mb-0">Welcome Back</h2>
              <p className="text-white text-opacity-75 small">Access your tickets and bookings</p>
            </div>
            <Card.Body className="p-4 p-md-5">
              {error && (
                <Alert variant="danger" className="py-2 border-0 bg-danger bg-opacity-10 text-danger d-flex align-items-center gap-2 mb-4">
                  <Info size={16} /> {error}
                </Alert>
              )}

              <Form onSubmit={handleLogin}>
                <Form.Group className="mb-4">
                  <Form.Label className="text-secondary small fw-bold text-uppercase">Email Address</Form.Label>
                  <InputGroup className="shadow-sm">
                    <InputGroup.Text className="bg-dark border-secondary text-secondary">
                      <Mail size={18} />
                    </InputGroup.Text>
                    <Form.Control
                      type="email"
                      required
                      placeholder="name@example.com"
                      className="bg-dark border-secondary text-white shadow-none py-2"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                    />
                  </InputGroup>
                </Form.Group>

                <Form.Group className="mb-4">
                  <Form.Label className="text-secondary small fw-bold text-uppercase">Password</Form.Label>
                  <InputGroup className="shadow-sm">
                    <InputGroup.Text className="bg-dark border-secondary text-secondary">
                      <Lock size={18} />
                    </InputGroup.Text>
                    <Form.Control
                      type="password"
                      required
                      placeholder="••••••••"
                      className="bg-dark border-secondary text-white shadow-none py-2"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                    />
                  </InputGroup>
                </Form.Group>

                <Button
                  type="submit"
                  variant="primary"
                  className="w-100 py-3 fw-bold rounded-pill shadow-sm mt-2"
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <Spinner animation="border" size="sm" className="me-2" />
                      Logging in...
                    </>
                  ) : (
                    'Login to Account'
                  )}
                </Button>
              </Form>

              <div className="text-center mt-4 pt-3 border-top border-secondary border-opacity-25">
                <p className="text-secondary small mb-0">
                  Don't have an account? <Link to="/register" className="text-primary text-decoration-none fw-bold">Sign up free</Link>
                </p>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
}
