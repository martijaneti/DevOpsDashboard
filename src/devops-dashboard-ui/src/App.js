import { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import PipelinesPanel from './components/PipelinesPanel';
import HardwarePanel from './components/HardwarePanel';
import ErrorsPanel from './components/ErrorsPanel';
import './App.css';

const API_URL = 'http://localhost:5291';

export default function App() {
  const [connection, setConnection] = useState(null);
  const [connected, setConnected] = useState(false);
  const [pipelines, setPipelines] = useState([]);
  const [hardware, setHardware] = useState([]);
  const [errors, setErrors] = useState([]);

  // Initial data fetch
  useEffect(() => {
    fetch(`${API_URL}/api/pipelines/kunow`)
      .then(r => r.json())
      .then(setPipelines);

    fetch(`${API_URL}/api/hardware`)
      .then(r => r.json())
      .then(setHardware);

    fetch(`${API_URL}/api/monitor/errors`)
      .then(r => r.json())
      .then(setErrors);
  }, []);
  
  // Poll recent pipelines every 30s
  useEffect(() => {
    const interval = setInterval(() => {
    fetch(`${API_URL}/api/pipelines/kunow`)
      .then(r => r.json())
      .then(setPipelines);
    }, 30000);

    return () => clearInterval(interval);
    }, []);

  // SignalR connection
  useEffect(() => {
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/dashboard`)
      .withAutomaticReconnect()
      .build();

    conn.on('PipelinesUpdated', (data) => {
        if (data && data.length > 0) setPipelines(data);
        });
    conn.on('HardwareUpdated', setHardware);
    conn.on('ErrorsUpdated', setErrors);

    conn.start()
      .then(() => setConnected(true))
      .catch(err => console.error('SignalR error:', err));

    conn.onreconnecting(() => setConnected(false));
    conn.onreconnected(() => setConnected(true));

    setConnection(conn);

    return () => conn.stop();
  }, []);

  return (
    <div className="app">
      <header className="header">
        <div className="header-left">
          <h1>DevOps Dashboard</h1>
          <span className="subtitle">Real-time monitoring</span>
        </div>
        <div className={`status-badge ${connected ? 'connected' : 'disconnected'}`}>
          <span className="status-dot" />
          {connected ? 'Live' : 'Connecting...'}
        </div>
      </header>

      <main className="grid">
        <PipelinesPanel pipelines={pipelines} />
        <HardwarePanel hardware={hardware} />
        <ErrorsPanel errors={errors} />
      </main>
    </div>
  );
}