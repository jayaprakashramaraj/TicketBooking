import { useState } from 'react';
import API_BASE_URL from '../config';
import { PlusCircle, Info, CheckCircle } from 'lucide-react';

export default function Admin() {
  const [movieName, setMovieName] = useState('');
  const [theaterName, setTheaterName] = useState('');
  const [startTime, setStartTime] = useState('');
  const [price, setPrice] = useState('');
  const [status, setStatus] = useState<'idle' | 'saving' | 'success' | 'error'>('idle');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setStatus('saving');
    setError('');

    try {
      const response = await fetch(`${API_BASE_URL.CATALOG}/api/shows`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          movieName,
          theaterName,
          startTime,
          price: parseFloat(price)
        })
      });

      if (response.ok) {
        setStatus('success');
        setMovieName('');
        setTheaterName('');
        setStartTime('');
        setPrice('');
        setTimeout(() => setStatus('idle'), 3000);
      } else {
        setError('Failed to add show.');
        setStatus('error');
      }
    } catch (err) {
      setError('Connection failed.');
      setStatus('error');
    }
  };

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-3xl font-bold mb-8">Admin Dashboard</h1>
      
      <div className="bg-gray-800 rounded-2xl p-8 shadow-2xl">
        <h2 className="text-xl font-semibold mb-6 flex items-center">
          <PlusCircle className="mr-2 text-blue-400" size={24} />
          Add New Show
        </h2>

        {status === 'success' && (
          <div className="bg-green-900/50 border border-green-500 text-green-200 p-4 rounded-lg mb-6 flex items-center">
            <CheckCircle size={20} className="mr-2" />
            Show added successfully!
          </div>
        )}

        {error && (
          <div className="bg-red-900/50 border border-red-500 text-red-200 p-4 rounded-lg mb-6 flex items-center">
            <Info size={20} className="mr-2" />
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-gray-400 mb-2">Movie Name</label>
              <input 
                type="text" 
                required
                className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2.5 px-4 focus:ring-2 focus:ring-blue-500 outline-none"
                value={movieName}
                onChange={(e) => setMovieName(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-gray-400 mb-2">Theater Name</label>
              <input 
                type="text" 
                required
                className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2.5 px-4 focus:ring-2 focus:ring-blue-500 outline-none"
                value={theaterName}
                onChange={(e) => setTheaterName(e.target.value)}
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-gray-400 mb-2">Start Time</label>
              <input 
                type="datetime-local" 
                required
                className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2.5 px-4 focus:ring-2 focus:ring-blue-500 outline-none"
                value={startTime}
                onChange={(e) => setStartTime(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-gray-400 mb-2">Price ($)</label>
              <input 
                type="number" 
                step="0.01"
                required
                className="w-full bg-gray-900 border border-gray-700 rounded-lg py-2.5 px-4 focus:ring-2 focus:ring-blue-500 outline-none"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
              />
            </div>
          </div>

          <button 
            type="submit" 
            disabled={status === 'saving'}
            className={`w-full py-3 rounded-lg font-bold text-lg transition shadow-lg ${
              status === 'saving' ? 'bg-gray-700 text-gray-500 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700 text-white'
            }`}
          >
            {status === 'saving' ? 'Saving...' : 'Add Show'}
          </button>
        </form>
      </div>
    </div>
  );
}
