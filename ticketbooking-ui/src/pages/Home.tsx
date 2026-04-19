import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import API_BASE_URL from '../config';
import { Calendar, MapPin, Search } from 'lucide-react';
import { Row, Col, Card, Button, Form, InputGroup, Spinner } from 'react-bootstrap';

interface Show {
  id: string;
  movieName: string;
  theaterName: string;
  startTime: string;
  price: number;
}

export default function Home() {
  const [shows, setShows] = useState<Show[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetch(`${API_BASE_URL.CATALOG}/api/shows/search`)
      .then(res => res.json())
      .then(data => {
        setShows(data);
        setLoading(false);
      })
      .catch(err => {
        console.error(err);
        setLoading(false);
      });
  }, []);

  const filteredShows = shows.filter(show => 
    show.movieName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" variant="primary" />
        <p className="mt-3 text-secondary">Loading shows...</p>
      </div>
    );
  }

  return (
    <div className="fade-in">
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-center mb-5 gap-3">
        <div>
          <h1 className="display-5 fw-bold text-white mb-1">Featured Movies</h1>
          <p className="text-secondary">Discover the best shows in town</p>
        </div>
        <div style={{ maxWidth: '400px', width: '100%' }}>
          <InputGroup className="shadow-sm">
            <InputGroup.Text className="bg-dark border-secondary text-secondary">
              <Search size={20} />
            </InputGroup.Text>
            <Form.Control
              placeholder="Search for movies..."
              className="bg-dark border-secondary text-white shadow-none py-2"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </InputGroup>
        </div>
      </div>

      <Row xs={1} md={2} lg={3} className="g-4">
        {filteredShows.map(show => (
          <Col key={show.id}>
            <Card className="h-100 overflow-hidden shadow-sm border-0">
              <div 
                className="ratio ratio-16x9 d-flex align-items-center justify-content-center"
                style={{ 
                  background: 'linear-gradient(45deg, #1e293b 0%, #334155 100%)',
                  borderBottom: '1px solid rgba(255,255,255,0.05)'
                }}
              >
                <div className="p-4 text-center">
                   <h2 className="h3 fw-bold text-white text-uppercase tracking-tight mb-0">{show.movieName}</h2>
                </div>
              </div>
              <Card.Body className="p-4 d-flex flex-column">
                <div className="mb-3">
                  <div className="d-flex align-items-center text-secondary mb-2">
                    <MapPin size={16} className="me-2 text-primary" />
                    <span className="small">{show.theaterName}</span>
                  </div>
                  <div className="d-flex align-items-center text-secondary">
                    <Calendar size={16} className="me-2 text-primary" />
                    <span className="small">{new Date(show.startTime).toLocaleString(undefined, {
                      dateStyle: 'medium',
                      timeStyle: 'short'
                    })}</span>
                  </div>
                </div>
                
                <div className="mt-auto d-flex justify-content-between align-items-center pt-3 border-top border-secondary border-opacity-25">
                  <div className="text-success fw-bold fs-4">${show.price}</div>
                  <Button 
                    as={Link as any} 
                    to={`/book/${show.id}`} 
                    variant="primary" 
                    className="rounded-pill px-4 fw-bold shadow-sm"
                  >
                    Book Now
                  </Button>
                </div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>

      {filteredShows.length === 0 && (
        <div className="text-center py-5">
          <div className="mb-3 text-secondary opacity-50">
            <Search size={64} />
          </div>
          <h3 className="text-secondary">No movies found</h3>
          <p className="text-muted small">Try a different search term</p>
        </div>
      )}
    </div>
  );
}
