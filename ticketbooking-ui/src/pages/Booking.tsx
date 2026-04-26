import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import API_BASE_URL from '../config';
import { Armchair, CheckCircle, Info, Ticket, Users } from 'lucide-react';
import { Container, Row, Col, Card, Button, Spinner, Alert, Badge } from 'react-bootstrap';

interface Show {
  id: string;
  movieName: string;
  theaterName: string;
  startTime: string;
  price: number;
}

export default function Booking() {
  const { showId } = useParams();
  const [show, setShow] = useState<Show | null>(null);
  const [bookedSeats, setBookedSeats] = useState<string[]>([]);
  const [selectedSeats, setSelectedSeats] = useState<string[]>([]);
  const [bookingStatus, setBookingStatus] = useState<'idle' | 'booking' | 'processing' | 'success' | 'error'>('idle');
  const [lastBookingId, setLastBookingId] = useState<string | null>(null);
  const [pdfStatus, setPdfStatus] = useState<'waiting' | 'generating' | 'ready'>('waiting');
  const [finalBookingDetails, setFinalBookingDetails] = useState<{ seats: string[], total: number } | null>(null);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const seats = Array.from({ length: 40 }, (_, i) => `${Math.floor(i / 10) + 1}${String.fromCharCode(65 + (i % 10))}`);

  const pollPdfStatus = async (bookingId: string) => {
    setPdfStatus('generating');
    const interval = setInterval(async () => {
      try {
        const response = await fetch(`${API_BASE_URL.NOTIFICATION}/api/tickets/${bookingId}`, { method: 'HEAD' });
        if (response.ok) {
          clearInterval(interval);
          setPdfStatus('ready');
        }
      } catch (err) {
        console.error('PDF polling error:', err);
      }
    }, 2000);

    // Stop after 2 minutes
    setTimeout(() => clearInterval(interval), 120000);
  };

  const pollBookingStatus = async (bookingId: string) => {
    const interval = setInterval(async () => {
      try {
        const response = await fetch(`${API_BASE_URL.BOOKING}/api/bookings/${bookingId}/status`);
        if (response.ok) {
          const data = await response.json();
          // Backend returns string names (Pending, Confirmed, Cancelled)
          const isConfirmed = data.status === 'Confirmed' || data.status === 1;
          const isCancelled = data.status === 'Cancelled' || data.status === 2;

          if (isConfirmed) {
            clearInterval(interval);
            setLastBookingId(bookingId);
            setBookingStatus('success');
            pollPdfStatus(bookingId);
          } else if (isCancelled) {
            clearInterval(interval);
            setError('Booking was cancelled.');
            setBookingStatus('error');
          }
          // Continue polling if 'Pending' or 'Processing'
        }
      } catch (err) {
        console.error('Polling error:', err);
      }
    }, 2000); // Poll every 2 seconds

    // Stop polling after 30 seconds (timeout)
    setTimeout(() => {
      clearInterval(interval);
      if (bookingStatus === 'processing') {
        setError('Booking is taking longer than expected. Please check your email later.');
        setBookingStatus('error');
      }
    }, 30000);
  };

  useEffect(() => {
    // Fetch show details
    fetch(`${API_BASE_URL.CATALOG}/api/shows/${showId}`)
      .then(res => res.json())
      .then(data => setShow(data))
      .catch(err => console.error(err));

    // Fetch booked seats
    fetch(`${API_BASE_URL.BOOKING}/api/bookings/shows/${showId}/seats`)
      .then(res => res.json())
      .then(data => setBookedSeats(data))
      .catch(err => console.error(err));
  }, [showId]);

  const toggleSeat = (seat: string) => {
    if (bookedSeats.includes(seat)) return;
    setSelectedSeats(prev => 
      prev.includes(seat) ? prev.filter(s => s !== seat) : [...prev, seat]
    );
  };

  const handleBooking = async () => {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      navigate('/login');
      return;
    }

    const user = JSON.parse(userStr);
    setBookingStatus('booking');
    setError('');

    // Capture details before they might get cleared
    setFinalBookingDetails({
      seats: [...selectedSeats],
      total: selectedSeats.length * (show?.price || 0)
    });

    try {
      const response = await fetch(`${API_BASE_URL.BOOKING}/api/bookings`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          showId: show?.id,
          showName: show?.movieName,
          showTime: show?.startTime,
          customerEmail: user.email,
          seatNumbers: selectedSeats,
          totalAmount: (show?.price || 0) * selectedSeats.length,
          redirectUrl: `${window.location.origin}/booking-result`
        })
      });

      if (response.status === 202) {
        setBookingStatus('processing');
        const data = await response.json();
        setLastBookingId(data.bookingId);
        
        if (data.paymentUrl) {
          // Redirect to payment simulator
          window.location.href = data.paymentUrl;
        } else {
          pollBookingStatus(data.bookingId);
        }
      } else if (response.ok) {
        setBookingStatus('success');
      } else if (response.status === 409) {
        setError('These seats were just taken! Please select others.');
        setBookingStatus('error');
      } else {
        const msg = await response.text();
        setError(msg || 'Booking failed.');
        setBookingStatus('error');
      }
    } catch (err) {
      setError('Connection failed. Please try again.');
      setBookingStatus('error');
    }
  };

  if (!show) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
        <p className="mt-3 text-secondary">Loading show details...</p>
      </div>
    );
  }

  if (bookingStatus === 'success') {
    return (
      <Container className="py-5">
        <Row className="justify-content-center">
          <Col md={8} lg={6}>
            <Card className="shadow-lg border-0 bg-dark text-white rounded-3 overflow-hidden">
              <div className="bg-success py-4 text-center">
                <CheckCircle size={64} className="text-white mb-2" />
                <h2 className="fw-bold mb-0">Booking Confirmed!</h2>
              </div>
              <Card.Body className="p-4 p-md-5">
                <div className="mb-4 text-center">
                  <h3 className="text-primary fw-bold mb-1">{show.movieName}</h3>
                  <p className="text-secondary">{show.theaterName}</p>
                </div>

                <div className="bg-black bg-opacity-40 rounded-4 p-4 mb-4 border border-secondary border-opacity-20 shadow-inner">
                  <Row className="gy-4">
                    <Col xs={12} md={6}>
                      <div className="d-flex flex-column">
                        <label className="text-primary small text-uppercase fw-bold ls-1 mb-1">Date & Time</label>
                        <div className="fw-medium text-white-50">{new Date(show.startTime).toLocaleString()}</div>
                      </div>
                    </Col>
                    <Col xs={12} md={6}>
                      <div className="d-flex flex-column">
                        <label className="text-primary small text-uppercase fw-bold ls-1 mb-1">Seats Reserved</label>
                        <div className="fw-bold text-success fs-5">{finalBookingDetails?.seats.join(', ')}</div>
                      </div>
                    </Col>
                    <Col xs={12} md={6}>
                      <div className="d-flex flex-column">
                        <label className="text-primary small text-uppercase fw-bold ls-1 mb-1">Amount Paid</label>
                        <div className="fw-bold text-white fs-5">${finalBookingDetails?.total}</div>
                      </div>
                    </Col>
                    <Col xs={12} md={6}>
                      <div className="d-flex flex-column">
                        <label className="text-primary small text-uppercase fw-bold ls-1 mb-1">Booking Reference</label>
                        <div className="text-white-50 font-monospace small text-truncate" title={lastBookingId || ''}>
                          {lastBookingId}
                        </div>
                      </div>
                    </Col>
                  </Row>
                </div>

                <div className="d-grid gap-3">
                  <Button 
                    href={`${API_BASE_URL.NOTIFICATION}/api/tickets/${lastBookingId}`}
                    target="_blank"
                    variant="primary" 
                    size="lg" 
                    disabled={pdfStatus !== 'ready'}
                    className="rounded-pill fw-bold"
                  >
                    {pdfStatus === 'ready' ? (
                      <>Download PDF Ticket</>
                    ) : (
                      <>
                        <Spinner animation="border" size="sm" className="me-2" />
                        Generating PDF Ticket...
                      </>
                    )}
                  </Button>
                  <Button 
                    variant="outline-secondary" 
                    onClick={() => navigate('/')}
                    className="rounded-pill"
                  >
                    Back to Home
                  </Button>
                </div>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </Container>
    );
  }

  return (
    <Container className="py-2">
      <Row className="mb-4 align-items-end">
        <Col md={8}>
          <div className="d-flex align-items-center gap-3 mb-2">
            <Ticket className="text-primary" size={32} />
            <h1 className="display-6 fw-bold text-white mb-0">{show.movieName}</h1>
          </div>
          <p className="text-secondary fs-5 mb-0">
            {show.theaterName} • {new Date(show.startTime).toLocaleString(undefined, { 
              dateStyle: 'full', 
              timeStyle: 'short' 
            })}
          </p>
        </Col>
        <Col md={4} className="text-md-end mt-3 mt-md-0">
          <Badge bg="success" className="p-3 fs-5 rounded-pill">
            ${show.price} per seat
          </Badge>
        </Col>
      </Row>

      <Row className="g-4">
        <Col lg={8}>
          <Card className="shadow-lg border-0 mb-4 bg-dark text-white rounded-3">
            <Card.Body className="p-4 p-md-5">
              <div className="mb-5">
                <div className="screen-divider"></div>
                <p className="text-center text-uppercase small tracking-widest text-secondary mt-3">Theater Screen</p>
              </div>

              <div className="d-flex flex-wrap justify-content-center gap-3 mb-5">
                {seats.map(seat => {
                  const isOccupied = bookedSeats.includes(seat);
                  const isSelected = selectedSeats.includes(seat);
                  return (
                    <button
                      key={seat}
                      onClick={() => toggleSeat(seat)}
                      disabled={isOccupied}
                      className={`seat ${isSelected ? 'selected' : ''} ${isOccupied ? 'occupied' : ''}`}
                      aria-label={`Seat ${seat}`}
                    >
                      <div className="d-flex flex-column align-items-center">
                        <Armchair size={18} />
                        <span className="small fw-bold" style={{ fontSize: '0.65rem' }}>{seat}</span>
                      </div>
                    </button>
                  );
                })}
              </div>

              <div className="d-flex flex-wrap justify-content-center gap-4 py-4 border-top border-secondary border-opacity-25">
                <div className="d-flex align-items-center gap-2">
                  <div className="seat" style={{ width: '20px', height: '20px', cursor: 'default' }}></div>
                  <span className="small text-secondary">Available</span>
                </div>
                <div className="d-flex align-items-center gap-2">
                  <div className="seat selected" style={{ width: '20px', height: '20px', cursor: 'default' }}></div>
                  <span className="small text-secondary">Selected</span>
                </div>
                <div className="d-flex align-items-center gap-2">
                  <div className="seat occupied" style={{ width: '20px', height: '20px', cursor: 'default' }}></div>
                  <span className="small text-secondary">Occupied</span>
                </div>
              </div>
            </Card.Body>
          </Card>
        </Col>

        <Col lg={4}>
          <Card className="shadow-lg border-0 sticky-top bg-dark text-white rounded-3" style={{ top: '100px' }}>
            <Card.Header className="bg-primary text-white p-4 fw-bold border-0 fs-5 rounded-top-3">
              Booking Summary
            </Card.Header>
            <Card.Body className="p-4">
              <div className="mb-4">
                <div className="d-flex justify-content-between mb-2">
                  <span className="text-secondary">Selected Seats:</span>
                  <span className="fw-bold text-white">{selectedSeats.length > 0 ? selectedSeats.join(', ') : 'None'}</span>
                </div>
                <div className="d-flex justify-content-between mb-4 pt-3 border-top border-secondary">
                  <span className="text-secondary fs-4">Total:</span>
                  <span className="fw-bold fs-4 text-success">${selectedSeats.length * show.price}</span>
                </div>
              </div>

              {error && (
                <Alert variant="danger" className="py-2 border-0 bg-danger bg-opacity-10 text-danger d-flex align-items-center gap-2">
                  <Info size={16} /> {error}
                </Alert>
              )}

              <Button
                disabled={selectedSeats.length === 0 || bookingStatus === 'booking' || bookingStatus === 'processing'}
                onClick={handleBooking}
                size="lg"
                variant="primary"
                className="w-100 py-3 fw-bold rounded-pill shadow-sm"
              >
                {bookingStatus === 'booking' ? (
                  <>
                    <Spinner animation="border" size="sm" className="me-2" />
                    Reserving Seats...
                  </>
                ) : bookingStatus === 'processing' ? (
                  <>
                    <Spinner animation="border" size="sm" className="me-2" />
                    Processing Payment...
                  </>
                ) : (
                  <>
                    Confirm My Booking
                  </>
                )}
              </Button>
              
              <p className="text-center small text-secondary mt-3 mb-0">
                <Users size={14} className="me-1" /> Join 500+ users today
              </p>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
}
