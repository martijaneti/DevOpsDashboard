import './ErrorsPanel.css';

const severityConfig = {
  Critical: { color: '#ef4444', bg: 'rgba(239,68,68,0.1)' },
  Error:    { color: '#f97316', bg: 'rgba(249,115,22,0.1)' },
  Warning:  { color: '#eab308', bg: 'rgba(234,179,8,0.1)'  },
};

function timeAgo(timestamp) {
  const seconds = Math.floor((new Date() - new Date(timestamp)) / 1000);
  if (seconds < 60) return `${seconds}s ago`;
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
  return `${Math.floor(seconds / 3600)}h ago`;
}

export default function ErrorsPanel({ errors }) {
  const critical = errors.filter(e => e.severity === 'Critical').length;
  const total = errors.reduce((sum, e) => sum + e.count, 0);

  return (
    <div className="panel errors-panel">
      <div className="panel-header">
        <h2>Application Errors</h2>
        <div className="errors-summary">
          {critical > 0 && (
            <span className="summary-badge critical">{critical} critical</span>
          )}
          <span className="summary-badge total">{total} total</span>
        </div>
      </div>

      {errors.length === 0 ? (
        <div className="empty">No errors in the last hour 🎉</div>
      ) : (
        <div className="errors-list">
          {errors.map((error, i) => {
            const config = severityConfig[error.severity] || severityConfig.Error;
            return (
              <div key={i} className="error-row">
                <div className="error-left">
                  <span
                    className="severity-badge"
                    style={{ background: config.bg, color: config.color }}
                  >
                    {error.severity}
                  </span>
                  <div className="error-info">
                    <span className="error-message">{error.message}</span>
                    <span className="error-meta">
                      {error.source} · {timeAgo(error.timestamp)}
                    </span>
                  </div>
                </div>
                <span className="error-count">×{error.count}</span>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}