import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { debugLog, debugError } from '../utils/debug';
import { getHealthCheckUrl, getServicePorts, getApiEndpoint } from '../config/ports';
import { useBranding } from '../contexts/BrandingContext';
import '../styles/Footer.css';

interface HealthStatus {
  status: 'up' | 'down';
  timestamp?: string;
}

interface DemoStatus {
  useDemoDatabase: boolean;
  demoDataSeeded: boolean;
  demoDataLastSeeded?: string;
}

interface VersionInfo {
  major: number;
  minor: number;
  patch: number;
  lastUpdate: string;
  components?: {
    api?: { version: string; build: string; lastDeployed: string };
    frontend?: { version: string; build: string; lastDeployed: string };
    database?: { version: string; schema: string; zipCodes: number; countries: number };
  };
  git?: { branch: string; commit: string };
  buildServer?: string;
}

function Footer() {
  const [apiStatus, setApiStatus] = useState<HealthStatus>({ status: 'down' });
  const [dbStatus, setDbStatus] = useState<HealthStatus>({ status: 'down' });
  const [demoStatus, setDemoStatus] = useState<DemoStatus | null>(null);
  const [ports, setPorts] = useState(getServicePorts());
  const { branding } = useBranding();
  const [versionInfo, setVersionInfo] = useState<VersionInfo | null>(null);
  const [buildInfo, setBuildInfo] = useState({
    version: process.env.REACT_APP_VERSION || '1.5.0',
    buildDate: process.env.REACT_APP_BUILD_DATE || new Date().toISOString().split('T')[0],
  });

  // Fetch version info from API
  useEffect(() => {
    const fetchVersionInfo = async () => {
      try {
        const response = await fetch('/version.json');
        if (response.ok) {
          const data = await response.json();
          setVersionInfo(data);
          setBuildInfo({
            version: `${data.major}.${data.minor}.${data.patch}`,
            buildDate: data.lastUpdate || new Date().toISOString().split('T')[0],
          });
        }
      } catch (err) {
        debugError('Failed to fetch version info', err);
      }
    };

    fetchVersionInfo();
  }, []);

  useEffect(() => {
    // Cache the port configuration
    setPorts(getServicePorts());
  }, []);

  // Fetch demo database status
  useEffect(() => {
    const fetchDemoStatus = async () => {
      try {
        const token = localStorage.getItem('accessToken');
        if (!token) return;
        
        const response = await fetch(getApiEndpoint('/systemsettings/demo/status'), {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (response.ok) {
          const data = await response.json();
          setDemoStatus(data);
        }
      } catch (err) {
        debugError('Failed to fetch demo status', err);
      }
    };

    fetchDemoStatus();
    // Refresh demo status every 60 seconds
    const interval = setInterval(fetchDemoStatus, 60000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const checkHealth = async () => {
      try {
        const healthUrl = getHealthCheckUrl();
        
        debugLog('Health check URL:', { healthUrl, ports });
        
        const response = await axios.get(healthUrl, {
          timeout: 5000,
        });
        
        debugLog('Health check success:', response.data);
        
        if (response.status === 200) {
          setApiStatus({ status: 'up', timestamp: response.data.timestamp });
          // If API is up, database is also up (API checks DB health)
          setDbStatus({ status: 'up' });
        }
      } catch (error: any) {
        debugError('Health check failed:', {
          message: error.message,
          status: error.response?.status,
        });
        setApiStatus({ status: 'down' });
        setDbStatus({ status: 'down' });
      }
    };

    // Check health immediately and then every 30 seconds
    checkHealth();
    const interval = setInterval(checkHealth, 30000);

    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const getStatusClass = (status: 'up' | 'down') => {
    return status === 'up' ? 'status-up' : 'status-down';
  };

  const getStatusText = (status: 'up' | 'down') => {
    return status === 'up' ? '●' : '○';
  };

  return (
    <footer className="footer-compact">
      <div className="footer-content-compact">
        <div className="footer-left">
          <span className="company-name">{branding.companyName || 'CRM System'}</span>
          <span className="separator">|</span>
          <span className="version" title={versionInfo ? `API: ${versionInfo.components?.api?.build || 'N/A'} | Frontend: ${versionInfo.components?.frontend?.build || 'N/A'} | DB: ${versionInfo.components?.database?.zipCodes || 0} ZIPs` : ''}>
            v{buildInfo.version}
          </span>
          {versionInfo?.git?.commit && (
            <>
              <span className="separator">|</span>
              <span className="git-info" title={`Branch: ${versionInfo.git.branch}`}>
                #{versionInfo.git.commit}
              </span>
            </>
          )}
        </div>

        <div className="footer-center">
          <div className="status-compact">
            <span className={`status-dot ${getStatusClass(apiStatus.status)}`}>{getStatusText(apiStatus.status)}</span>
            <span className="status-text" title={versionInfo?.components?.api?.build ? `Build: ${versionInfo.components.api.build}` : ''}>
              API {versionInfo?.components?.api?.build ? `(${versionInfo.components.api.build})` : ''}
            </span>
            <span className={`status-dot ${getStatusClass(dbStatus.status)}`}>{getStatusText(dbStatus.status)}</span>
            <span className="status-text" title={versionInfo?.components?.database ? `${versionInfo.components.database.zipCodes} ZIPs, ${versionInfo.components.database.countries} Countries` : ''}>
              DB
            </span>
            {demoStatus && (
              <>
                <span className="separator">|</span>
                <span 
                  className={`db-mode ${demoStatus.useDemoDatabase ? 'demo-mode' : 'prod-mode'}`}
                  title={demoStatus.useDemoDatabase && demoStatus.demoDataLastSeeded 
                    ? `Demo data seeded: ${new Date(demoStatus.demoDataLastSeeded).toLocaleDateString()}` 
                    : 'Production database'}
                >
                  {demoStatus.useDemoDatabase ? '🧪 Demo' : '🏭 Prod'}
                </span>
              </>
            )}
          </div>
        </div>

        <div className="footer-right">
          <Link to="/about" className="footer-link">About</Link>
          <span className="separator">|</span>
          <Link to="/help" className="footer-link">Help</Link>
          <span className="separator">|</span>
          <Link to="/licenses" className="footer-link">Licenses</Link>
          <span className="separator">|</span>
          <span className="copyright">&copy; 2026 Abhishek Lal - AGPL-3.0</span>
        </div>
      </div>
    </footer>
  );
}

export default Footer;
