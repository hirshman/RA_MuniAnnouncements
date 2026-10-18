export interface NormalizedRequest {
  requestId: string;
  serviceName: string;
  authorityName: string;
  status: string;
  actionRequired: boolean;
  isOverdue: boolean;
  submittedAt: string;
  lastUpdate: string;
}
