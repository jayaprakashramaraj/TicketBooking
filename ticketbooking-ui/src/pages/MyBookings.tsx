import { useState, useEffect } from 'react';
import API_BASE_URL from '../config';
import { Ticket, Calendar, Clock, Armchair, History } from 'lucide-react';
import { Container, Row, Col, Card, Badge, Spinner, Alert, Button } from 'react-bootstrap';
import { Link } from 'react-router-dom';

interface Booking {
  id: string;
  showName: string;
  showTime: string;
  seatNumbers: string[];
  totalAmount: number;
  status: string | number;
}

export default function MyBookings() {
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  const userStr = localStorage.getItem('user');
  const user = userStr ? JSON.parse(userStr) : {};

  useEffect(() => {
    const fetchBookings = async () => {
      if (!user.email) {
        setError('Please login to view your bookings.');
        setLoading(false);
        return;
      }

      try {
        const response = await fetch(`${API_BASE_URL.BOOKING}/api/bookings/user/${user.email}`);
        if (response.ok) {
          const data = await response.json();
          setBookings(data);
        } else {
          setError('Failed to load bookings.');
        }
      } catch (err) {
        setError('Connection error. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchBookings();
  }, [user.email]);

  const now = new Date();
  const upcoming = bookings.filter(b => new Date(b.showTime) >= now && b.status !== 'Cancelled');
  const past = bookings.filter(b => (new Date(b.showTime) < now && b.status !== 'Pending') || b.status === 'Cancelled');

  const BookingCard = ({ booking }: { booking: Booking }) => (
    <Card className="mb-3 bg-dark border-secondary border-opacity-25 shadow-sm overflow-hidden">
      <Card.Body className="p-0">
        <div className="d-flex flex-column flex-md-row">
          <div className="bg-primary p-4 d-flex align-items-center justify-content-center" style={{ minWidth: '100px' }}>
            <Ticket className="text-white" size={32} />
          </div>
          <div className="p-4 flex-grow-1">
            <div className="d-flex justify-content-between align-items-start mb-2">
              <h4 className="fw-bold text-white mb-0">{booking.showName}</h4>
              <Badge bg={booking.status === 'Confirmed' ? 'success' : booking.status === 'Cancelled' ? 'danger' : 'warning'}>
                {booking.status}
              </Badge>
            </div>
            
            <Row className="text-secondary small g-3">
              <Col sm={6} md={4} className="d-flex align-items-center gap-2">
                <Calendar size={14} className="text-primary" />
                {new Date(booking.showTime).toLocaleDateString()}
              </Col>
              <Col sm={6} md={4} className="d-flex align-items-center gap-2">
                <Clock size={14} className="text-primary" />
                {new Date(booking.showTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </Col>
              <Col sm={12} md={4} className="d-flex align-items-center gap-2">
                <Armchair size={14} className="text-primary" />
                Seats: {booking.seatNumbers.join(', ')}
              </Col>
            </Row>
          </div>
          <div className="p-4 bg-black bg-opacity-40 d-flex flex-column align-items-center justify-content-center border-start border-secondary border-opacity-10" style={{ minWidth: '160px' }}>
            <div className="text-center mb-3">
              <div className="text-primary small text-uppercase fw-bold ls-1 mb-1">Total Paid</div>
              <div className="fw-bold text-white fs-4">${booking.totalAmount}</div>
            </div>
            {(booking.status === 'Confirmed' || booking.status === 1 || booking.status === 'Pending' || booking.status === 0) && new Date(booking.showTime).getTime() >= (now.getTime() - 3600000) && (
              <Button 
                href={`${API_BASE_URL.NOTIFICATION}/api/tickets/${booking.id}`}
                target="_blank"
                variant="outline-primary" 
                size="sm" 
                className="rounded-pill px-3 border-2 fw-bold"
              >
                Download PDF
              </Button>
            )}
          </div>
        </div>
      </Card.Body>
    </Card>
  );

  if (loading) {
    return (
      <Container className="text-center py-5">
        <Spinner animation="border" variant="primary" />
        <p className="mt-3 text-secondary">Loading your bookings...</p>
      </Container>
    );
  }

  return (
    <Container>
      <div className="mb-5">
        <h1 className="fw-bold text-white mb-2">My Bookings</h1>
        <p className="text-secondary">Manage your upcoming and past movie experiences</p>
      </div>

      {error && <Alert variant="danger" className="bg-danger bg-opacity-10 border-0 text-danger">{error}</Alert>}

      {!error && bookings.length === 0 ? (
        <div className="text-center py-5 bg-dark rounded-3 border border-secondary border-opacity-25">
          <Ticket size={48} className="text-secondary mb-3 opacity-25" />
          <h3 className="text-white">No bookings yet</h3>
          <p className="text-secondary mb-4">Ready to watch something amazing?</p>
          <Button as={Link as any} to="/" variant="primary" className="rounded-pill px-4">
            Browse Movies
          </Button>
        </div>
      ) : (
        <>
          {upcoming.length > 0 && (
            <div className="mb-5">
              <div className="d-flex align-items-center gap-2 mb-4">
                <div className="bg-primary bg-opacity-10 p-2 rounded">
                  <Calendar className="text-primary" size={20} />
                </div>
                <h3 className="fw-bold text-white mb-0">Upcoming Shows</h3>
              </div>
              {upcoming.map(b => <BookingCard key={b.id} booking={b} />)}
            </div>
          )}

          {past.length > 0 && (
            <div>
              <div className="d-flex align-items-center gap-2 mb-4">
                <div className="bg-secondary bg-opacity-10 p-2 rounded">
                  <History className="text-secondary" size={20} />
                </div>
                <h3 className="fw-bold text-white mb-0">Past & Cancelled</h3>
              </div>
              <div className="opacity-75">
                {past.map(b => <BookingCard key={b.id} booking={b} />)}
              </div>
            </div>
          )}
        </>
      )}
    </Container>
  );
}
