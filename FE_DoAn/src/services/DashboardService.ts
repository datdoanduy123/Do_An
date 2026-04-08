import api from './api';

export interface DashboardStats {
  totalProjects: number;
  completedTasks: number;
  inProgressTasks: number;
  pendingReviews: number;
  taskStatusDistribution: any[];
  teamWorkload: any[];
  sprintWorkload: any[];
  projectProgress: any[];
  burndownData: any[];
  velocityData: any[];
  recentProjects: any[];
  selectedProjectName?: string;
  myPriorityTasks: any[];
}

/**
 * Service tổng hợp dữ liệu cho Dashboard.
 */
class DashboardService {
  /**
   * Lấy số liệu thống kê cho Dashboard từ Backend tập trung.
   */
  async getDashboardData(projectId?: number): Promise<DashboardStats> {
    try {
      const response = await api.get('/Dashboard/stats', {
        params: { projectId }
      });
      const data = response.data.data;
      
      return {
        totalProjects: data.totalProjects,
        completedTasks: data.completedTasks,
        inProgressTasks: data.inProgressTasks,
        pendingReviews: data.pendingReviews,
        taskStatusDistribution: data.taskStatusDistribution,
        teamWorkload: data.teamWorkload,
        sprintWorkload: data.sprintWorkload,
        projectProgress: data.projectProgress,
        burndownData: data.burndownData,
        velocityData: data.velocityData,
        recentProjects: data.recentProjects,
        selectedProjectName: data.selectedProjectName,
        myPriorityTasks: data.myPriorityTasks
      };
    } catch (error) {
      console.error('Failed to fetch dashboard stats:', error);
      throw error;
    }
  }
}

export default new DashboardService();
