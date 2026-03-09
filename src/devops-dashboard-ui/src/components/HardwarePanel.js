import { useState } from 'react';
import './HardwarePanel.css';

const sensorGroups = {
  Temperature: { label: 'Temperatures', unit: '°C', color: '#f97316', max: 100 },
  Load:        { label: 'Load',         unit: '%',  color: '#3b82f6', max: 100 },
  Fan:         { label: 'Fan Speeds',   unit: 'RPM',color: '#8b5cf6', max: 3000 },
  Power:       { label: 'Power',        unit: 'W',  color: '#eab308', max: 200 },
};

function MetricBar({ name, value, max, color, unit }) {
  const pct = Math.min((value / max) * 100, 100);
  const isHigh = pct > 80;
  const isMed  = pct > 60;

  return (
    <div className="metric-bar">
      <div className="metric-bar-header">
        <span className="metric-name">{name}</span>
        <span className="metric-value" style={{ color: isHigh ? '#ef4444' : isMed ? '#f97316' : color }}>
          {value.toFixed(1)}{unit}
        </span>
      </div>
      <div className="bar-track">
        <div
          className="bar-fill"
          style={{
            width: `${pct}%`,
            background: isHigh ? '#ef4444' : isMed ? '#f97316' : color
          }}
        />
      </div>
    </div>
  );
}

export default function HardwarePanel({ hardware }) {
  const [activeType, setActiveType] = useState('Load');

  // Group by hardware type first, then sensor type
  const byHardware = hardware.reduce((acc, m) => {
    const key = m.hardwareName;
    if (!acc[key]) acc[key] = {};
    if (!acc[key][m.sensorType]) acc[key][m.sensorType] = [];
    acc[key][m.sensorType].push(m);
    return acc;
  }, {});

  const availableTypes = Object.keys(sensorGroups).filter(type =>
    Object.values(byHardware).some(h => h[type]?.length > 0)
  );

  return (
    <div className="panel hardware-panel">
      <div className="panel-header">
        <h2>Hardware Monitor</h2>
        <span className="panel-count">{hardware.length} sensors</span>
      </div>

      <div className="type-tabs">
        {availableTypes.map(type => (
          <button
            key={type}
            className={`tab ${activeType === type ? 'active' : ''}`}
            onClick={() => setActiveType(type)}
            style={{ '--tab-color': sensorGroups[type].color }}
          >
            {sensorGroups[type].label}
          </button>
        ))}
      </div>

      <div className="hardware-list">
        {Object.entries(byHardware).map(([hwName, sensorTypes]) => {
          const sensors = sensorTypes[activeType];
          if (!sensors?.length) return null;

          return (
            <div key={hwName} className="hardware-group">
              <div className="hardware-name">{hwName}</div>
              {sensors.map((s, i) => (
                <MetricBar
                  key={i}
                  name={s.sensorName}
                  value={s.value}
                  max={sensorGroups[activeType].max}
                  color={sensorGroups[activeType].color}
                  unit={sensorGroups[activeType].unit}
                />
              ))}
            </div>
          );
        })}
      </div>
    </div>
  );
}