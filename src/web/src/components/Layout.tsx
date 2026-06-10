import { useState } from 'react';
import { Link, Outlet, useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useEntityTypesWithEntities } from '../hooks/useEntityTypes';

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const [expandedTypes, setExpandedTypes] = useState<Set<string>>(new Set());
  const [entitiesExpanded, setEntitiesExpanded] = useState(false);

  const { data: entityTypes = [], isLoading } = useEntityTypesWithEntities();

  const selectedEntityId = searchParams.get('entity');
  const isOnReportsPage = location.pathname === '/reports';

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const toggleEntityType = (typeId: string) => {
    setExpandedTypes(prev => {
      const next = new Set(prev);
      if (next.has(typeId)) {
        next.delete(typeId);
      } else {
        next.add(typeId);
      }
      return next;
    });
  };

  const handleEntityClick = (entityId: string) => {
    navigate(`/?entity=${entityId}`);
  };

  const handleEntityTypeClick = (entityTypeId: string) => {
    navigate(`/?entityType=${entityTypeId}`);
  };

  const selectedEntityTypeId = searchParams.get('entityType');

  return (
    <div className="app-layout">
      <nav className="sidebar">
        <div className="sidebar-header">
          <h2>PropertyViewer</h2>
          <span>Accounting</span>
        </div>
        <div className="nav-menu">
          <Link to="/" className="nav-link">Dashboard</Link>

          <div className="nav-section">
            <div
              className="nav-entity-type-header nav-entities-header"
              onClick={() => setEntitiesExpanded(!entitiesExpanded)}
              style={{ cursor: 'pointer' }}
            >
              <span className={`nav-expand-icon ${entitiesExpanded ? 'expanded' : ''}`}>
                &#9658;
              </span>
              <span className="nav-entity-type-name">
                Entities
              </span>
            </div>
            {entitiesExpanded && (
              <div className="nav-entities-list">
                {isLoading ? (
                  <div className="nav-loading">Loading...</div>
                ) : (
                  entityTypes.map((entityType) => (
                    <div key={entityType.id} className="nav-entity-type">
                      <div
                        className={`nav-entity-type-header ${selectedEntityTypeId === entityType.id ? 'active' : ''}`}
                        style={{ borderLeftColor: entityType.color || '#6b7280' }}
                      >
                        <button
                          className="nav-expand-toggle"
                          onClick={() => toggleEntityType(entityType.id)}
                        >
                          <span className={`nav-expand-icon ${expandedTypes.has(entityType.id) ? 'expanded' : ''}`}>
                            &#9658;
                          </span>
                        </button>
                        <button
                          className="nav-entity-type-name"
                          onClick={() => handleEntityTypeClick(entityType.id)}
                        >
                          {entityType.name}
                        </button>
                      </div>
                      {expandedTypes.has(entityType.id) && (
                        <div className="nav-entity-list">
                          {entityType.entities.map((entity) => (
                            <button
                              key={entity.id}
                              className={`nav-entity-item ${selectedEntityId === entity.id ? 'active' : ''}`}
                              onClick={() => handleEntityClick(entity.id)}
                            >
                              {entity.name}
                            </button>
                          ))}
                        </div>
                      )}
                    </div>
                  ))
                )}
              </div>
            )}
          </div>

          <div className="nav-section">
            <Link to="/reports" className={`nav-link nav-reports-link ${isOnReportsPage ? 'active' : ''}`}>
              <span>Reports</span>
            </Link>
            <Link to="/integrations" className={`nav-link nav-integrations-link ${location.pathname === '/integrations' ? 'active' : ''}`}>
              <span>Integrations</span>
            </Link>
          </div>
        </div>
        <div className="sidebar-footer">
          <div className="user-info">
            <span>{user?.email}</span>
          </div>
          <button onClick={handleLogout} className="logout-btn">
            Sign Out
          </button>
        </div>
      </nav>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
