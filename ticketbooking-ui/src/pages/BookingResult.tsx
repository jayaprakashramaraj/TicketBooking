import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Card, Button, Spinner, Alert, ProgressBar } from 'react-bootstrap';
import { CheckCircle, XCircle, Clock, Download } from 'lucide-react';
import API_BASE_URL from '../config';

export default function BookingResult() {
    const [searchParams] = useSearchParams();
    const bookingId = searchParams.get('bookingId');
    const paymentStatus = searchParams.get('paymentStatus');
    
    const [bookingData, setBookingData] = useState<any>(null);
    const [ticketPdf, setTicketPdf] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [pdfProgress, setPdfProgress] = useState(0);

    const startPdfPolling = () => {
        let progress = 0;
        const progressInterval = setInterval(() => {
            progress += 5;
            if (progress <= 90) setPdfProgress(progress);
        }, 500);

        const interval = setInterval(async () => {
            try {
                const response = await fetch(`${API_BASE_URL.NOTIFICATION}/api/tickets/${bookingId}`, { method: 'HEAD' });
                if (response.ok) {
                    clearInterval(interval);
                    clearInterval(progressInterval);
                    setPdfProgress(100);
                    setTicketPdf(`${API_BASE_URL.NOTIFICATION}/api/tickets/${bookingId}`);
                }
            } catch (err) {
                console.error('PDF status error:', err);
            }
        }, 3000);
    };

    useEffect(() => {
        if (!bookingId) return;

        let pollCount = 0;
        const interval = setInterval(async () => {
            try {
                const response = await fetch(`${API_BASE_URL.BOOKING}/api/bookings/${bookingId}`);
                if (response.ok) {
                    const data = await response.json();
                    setBookingData(data);
                    
                    if (data.status === 'Confirmed') {
                        clearInterval(interval);
                        setLoading(false);
                        startPdfPolling();
                    } else if (data.status === 'Cancelled' || pollCount > 30) {
                        clearInterval(interval);
                        setLoading(false);
                    }
                }
            } catch (err) {
                console.error('Polling error:', err);
            }
            pollCount++;
        }, 2000);

        return () => clearInterval(interval);
    }, [bookingId]);

    if (paymentStatus === 'Failure' || paymentStatus === 'Cancel') {
        return (
            <div className="max-w-md mx-auto">
                <Card className="text-center border-danger bg-dark text-white p-4">
                    <XCircle size={64} className="text-danger mx-auto mb-3" />
                    <h2>Payment {paymentStatus === 'Cancel' ? 'Cancelled' : 'Failed'}</h2>
                    <p className="text-secondary mt-2">
                        Your payment was not completed. If money was deducted, it will be refunded.
                    </p>
                    <div className="d-grid gap-2 mt-4">
                        <Button as={Link as any} to="/" variant="outline-light">Return to Home</Button>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="max-w-2xl mx-auto">
            <Card className="bg-dark text-white border-secondary border-opacity-25 shadow-lg">
                <Card.Body className="p-5 text-center">
                    {loading ? (
                        <>
                            <Spinner animation="border" variant="primary" className="mb-4" style={{ width: '4rem', height: '4rem' }} />
                            <h2>Finalizing your booking...</h2>
                            <p className="text-secondary">Please don't close this window.</p>
                        </>
                    ) : bookingData?.status === 'Confirmed' ? (
                        <>
                            <CheckCircle size={80} className="text-success mx-auto mb-4" />
                            <h1 className="display-5 fw-bold mb-2">Booking Confirmed!</h1>
                            <p className="fs-5 text-secondary mb-4">
                                Your tickets for <strong>{bookingData.showName}</strong> are ready.
                            </p>

                            <Alert variant="info" className="bg-info bg-opacity-10 border-info border-opacity-25 text-info text-start mb-4">
                                <div className="d-flex align-items-center gap-3">
                                    <Clock size={24} />
                                    <div>
                                        <div className="fw-bold">Generating Ticket PDF</div>
                                        <div className="small opacity-75">We are preparing your official ticket for download.</div>
                                    </div>
                                </div>
                                <ProgressBar now={pdfProgress} variant="info" animated className="mt-3" style={{ height: '8px' }} />
                            </Alert>

                            <div className="d-flex flex-column flex-md-row gap-3 justify-content-center mt-5">
                                <Button 
                                    href={ticketPdf || '#'} 
                                    target="_blank" 
                                    variant="primary" 
                                    size="lg" 
                                    className="px-5 rounded-pill d-flex align-items-center justify-content-center gap-2"
                                    disabled={!ticketPdf}
                                >
                                    <Download size={20} />
                                    {ticketPdf ? 'Download Ticket' : 'Preparing PDF...'}
                                </Button>
                                <Button as={Link as any} to="/my-bookings" variant="outline-light" size="lg" className="px-5 rounded-pill">
                                    View My Bookings
                                </Button>
                            </div>
                        </>
                    ) : (
                        <>
                            <XCircle size={80} className="text-danger mx-auto mb-4" />
                            <h2>Something went wrong</h2>
                            <p>We couldn't confirm your booking status. Please check "My Bookings" or contact support.</p>
                            <Button as={Link as any} to="/my-bookings" variant="outline-light" className="mt-4">
                                Go to My Bookings
                            </Button>
                        </>
                    )}
                </Card.Body>
            </Card>
        </div>
    );
}
