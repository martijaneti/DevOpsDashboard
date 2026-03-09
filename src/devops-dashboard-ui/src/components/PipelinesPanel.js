import './PipelinesPanel.css';

const statusColors = {
  succeeded: { bg: 'rgba(34,197,94,0.1)', color: '#22c55e', label: 'Succeeded' },
  failed:    { bg: 'rgba(239,68,68,0.1)', color: '#ef4444', label: 'Failed' },
  running:   { bg: 'rgba(59,130,246,0.1)', color: '#3b82f6', label: 'Running' },
  canceled:  { bg: 'rgba(100,116,139,0.1)', color: '#64748b', label: 'Canceled' },
};

function StatusBadge({ status }) {
  const style = statusColors[status] || statusColors.canceled;
  return (
    <span className="badge" style={{ background: style.bg, color: style.color }}>
      {status === 'running' && <span className="spinner" />}
      {style.label}
    </span>
  );
}

function formatDuration(startTime, finishTime) {
  const start = new Date(startTime);
  const end = finishTime ? new Date(finishTime) : new Date();
  const seconds = Math.floor((end - start) / 1000);
  if (seconds < 60) return `${seconds}s`;
  return `${Math.floor(seconds / 60)}m ${seconds % 60}s`;
}

export default function PipelinesPanel({ pipelines }) {
  return (
    <div className="panel pipelines-panel">
      <div className="panel-header">
        <h2>CI/CD Pipelines</h2>
        <span className="panel-count">{pipelines.length} runs</span>
      </div>

      {pipelines.length === 0 ? (
        <div className="empty">No pipeline runs found</div>
      ) : (
        <div className="pipeline-list">
          {pipelines.map(run => (
            <div key={run.id} className="pipeline-row">
              <div className="pipeline-info">
                <span className="pipeline-name">{run.name}</span>
                <span className="pipeline-meta">
                  {run.project} · {run.branchName} · {run.triggeredBy}
                </span>
              </div>
              <div className="pipeline-right">
                <StatusBadge status={run.status} />
                <span className="pipeline-duration">
                  {formatDuration(run.startTime, run.finishTime)}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}