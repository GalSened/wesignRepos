import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit, OnChanges } from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { SegmentationData, SegmentBreakdown, OrganizationSegment, TemplateUsageData } from '@models/analytics/analytics-models';

Chart.register(...registerables);

@Component({
  selector: 'sgn-segmentation-charts',
  templateUrl: './segmentation-charts.component.html',
  styleUrls: ['./segmentation-charts.component.scss']
})
export class SegmentationChartsComponent implements OnInit, OnDestroy, AfterViewInit, OnChanges {
  @Input() segmentationData: SegmentationData = new SegmentationData();
  @Input() timeRange: string = '30d';

  @ViewChild('sendTypeChart', { static: false }) sendTypeChartRef: ElementRef<HTMLCanvasElement>;
  @ViewChild('deviceChart', { static: false }) deviceChartRef: ElementRef<HTMLCanvasElement>;
  @ViewChild('organizationChart', { static: false }) organizationChartRef: ElementRef<HTMLCanvasElement>;

  private sendTypeChart: Chart;
  private deviceChart: Chart;
  private organizationChart: Chart;

  // Chart color palettes
  private chartColors = {
    primary: ['#007bff', '#28a745', '#ffc107', '#dc3545', '#17a2b8', '#6f42c1', '#fd7e14'],
    success: ['#d4edda', '#28a745', '#155724'],
    performance: {
      excellent: '#28a745',
      good: '#ffc107',
      fair: '#fd7e14',
      poor: '#dc3545'
    }
  };

  ngOnInit(): void {
    // Component initialization
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.initializeCharts();
    }, 100);
  }

  ngOnDestroy(): void {
    if (this.sendTypeChart) {
      this.sendTypeChart.destroy();
    }
    if (this.deviceChart) {
      this.deviceChart.destroy();
    }
    if (this.organizationChart) {
      this.organizationChart.destroy();
    }
  }

  ngOnChanges(): void {
    if (this.sendTypeChart && this.deviceChart && this.organizationChart) {
      this.updateCharts();
    }
  }

  private initializeCharts(): void {
    if (this.sendTypeChartRef && this.deviceChartRef && this.organizationChartRef) {
      this.createSendTypeChart();
      this.createDeviceChart();
      this.createOrganizationChart();
    }
  }

  private createSendTypeChart(): void {
    const ctx = this.sendTypeChartRef.nativeElement.getContext('2d');

    const configuration: ChartConfiguration = {
      type: 'doughnut',
      data: {
        labels: this.segmentationData.sendTypeBreakdown.map(item => item.name),
        datasets: [
          {
            label: 'Documents by Send Type',
            data: this.segmentationData.sendTypeBreakdown.map(item => item.count),
            backgroundColor: this.segmentationData.sendTypeBreakdown.map((_, index) =>
              this.chartColors.primary[index % this.chartColors.primary.length]
            ),
            borderWidth: 2,
            borderColor: '#fff',
            hoverBorderWidth: 3,
            hoverOffset: 8
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          title: {
            display: true,
            text: 'Document Distribution by Send Type',
            font: {
              size: 14,
              weight: 'bold'
            },
            color: '#2c3e50',
            padding: {
              bottom: 20
            }
          },
          legend: {
            position: 'bottom',
            labels: {
              usePointStyle: true,
              padding: 20,
              font: {
                size: 11
              },
              generateLabels: (chart) => {
                const data = chart.data;
                return data.labels.map((label, index) => {
                  const breakdown = this.segmentationData.sendTypeBreakdown[index];
                  return {
                    text: `${label} (${breakdown.percentage}%)`,
                    fillStyle: data.datasets[0].backgroundColor[index],
                    strokeStyle: data.datasets[0].backgroundColor[index],
                    pointStyle: 'circle',
                    hidden: false,
                    index: index
                  };
                });
              }
            }
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: '#ddd',
            borderWidth: 1,
            cornerRadius: 8,
            callbacks: {
              label: (context) => {
                const breakdown = this.segmentationData.sendTypeBreakdown[context.dataIndex];
                return [
                  `Count: ${breakdown.count.toLocaleString()}`,
                  `Percentage: ${breakdown.percentage}%`,
                  `Success Rate: ${breakdown.successRate}%`,
                  `Avg Time: ${breakdown.averageTimeToSign}min`
                ];
              }
            }
          }
        },
        cutout: '60%',
        animation: {
          animateRotate: true,
          animateScale: true
        }
      }
    };

    this.sendTypeChart = new Chart(ctx, configuration);
  }

  private createDeviceChart(): void {
    const ctx = this.deviceChartRef.nativeElement.getContext('2d');

    const configuration: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: this.segmentationData.deviceBreakdown.map(item => item.name),
        datasets: [
          {
            label: 'Document Count',
            data: this.segmentationData.deviceBreakdown.map(item => item.count),
            backgroundColor: this.segmentationData.deviceBreakdown.map((_, index) =>
              this.chartColors.primary[index % this.chartColors.primary.length] + '40'
            ),
            borderColor: this.segmentationData.deviceBreakdown.map((_, index) =>
              this.chartColors.primary[index % this.chartColors.primary.length]
            ),
            borderWidth: 2,
            borderRadius: 6,
            borderSkipped: false
          },
          {
            label: 'Success Rate (%)',
            data: this.segmentationData.deviceBreakdown.map(item => item.successRate),
            type: 'line',
            borderColor: '#dc3545',
            backgroundColor: 'rgba(220, 53, 69, 0.1)',
            borderWidth: 3,
            fill: false,
            tension: 0.4,
            pointRadius: 6,
            pointHoverRadius: 8,
            yAxisID: 'y1'
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          mode: 'index',
          intersect: false,
        },
        plugins: {
          title: {
            display: true,
            text: 'Device Usage and Performance',
            font: {
              size: 14,
              weight: 'bold'
            },
            color: '#2c3e50'
          },
          legend: {
            position: 'top',
            labels: {
              usePointStyle: true,
              font: {
                size: 11
              }
            }
          },
          tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: '#ddd',
            borderWidth: 1,
            cornerRadius: 8,
            callbacks: {
              label: (context) => {
                const deviceData = this.segmentationData.deviceBreakdown[context.dataIndex];
                if (context.datasetIndex === 0) {
                  return `Documents: ${deviceData.count.toLocaleString()} (${deviceData.percentage}%)`;
                } else {
                  return `Success Rate: ${deviceData.successRate}%`;
                }
              },
              afterLabel: (context) => {
                if (context.datasetIndex === 0) {
                  const deviceData = this.segmentationData.deviceBreakdown[context.dataIndex];
                  return `Avg Time to Sign: ${deviceData.averageTimeToSign}min`;
                }
                return '';
              }
            }
          }
        },
        scales: {
          x: {
            display: true,
            title: {
              display: true,
              text: 'Device Type',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              display: false
            }
          },
          y: {
            type: 'linear',
            display: true,
            position: 'left',
            title: {
              display: true,
              text: 'Document Count',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              color: 'rgba(0, 0, 0, 0.1)'
            },
            beginAtZero: true
          },
          y1: {
            type: 'linear',
            display: true,
            position: 'right',
            title: {
              display: true,
              text: 'Success Rate (%)',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              drawOnChartArea: false,
            },
            min: 0,
            max: 100
          }
        }
      }
    };

    this.deviceChart = new Chart(ctx, configuration);
  }

  private createOrganizationChart(): void {
    const ctx = this.organizationChartRef.nativeElement.getContext('2d');

    // Sort organizations by document count for better visualization
    const sortedOrgs = [...this.segmentationData.organizationBreakdown]
      .sort((a, b) => b.documentsCount - a.documentsCount)
      .slice(0, 8); // Show top 8 organizations

    const configuration: ChartConfiguration = {
      type: 'scatter',
      data: {
        datasets: [
          {
            label: 'Organizations',
            data: sortedOrgs.map(org => ({
              x: org.usersCount,
              y: org.documentsCount,
              organizationName: org.organizationName,
              successRate: org.successRate,
              tier: org.tier
            })),
            backgroundColor: sortedOrgs.map(org =>
              this.getOrganizationColor(org.tier)
            ),
            borderColor: sortedOrgs.map(org =>
              this.getOrganizationColor(org.tier)
            ),
            borderWidth: 2,
            pointRadius: sortedOrgs.map(org =>
              Math.max(6, Math.min(16, Math.sqrt(org.documentsCount) / 5))
            ),
            pointHoverRadius: sortedOrgs.map(org =>
              Math.max(8, Math.min(20, Math.sqrt(org.documentsCount) / 4))
            )
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          title: {
            display: true,
            text: 'Organizations: Users vs Documents (Bubble Size = Volume)',
            font: {
              size: 14,
              weight: 'bold'
            },
            color: '#2c3e50'
          },
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: '#ddd',
            borderWidth: 1,
            cornerRadius: 8,
            callbacks: {
              title: (tooltipItems) => {
                return tooltipItems[0].raw.organizationName;
              },
              label: (context) => {
                const data = context.raw;
                return [
                  `Users: ${data.x.toLocaleString()}`,
                  `Documents: ${data.y.toLocaleString()}`,
                  `Success Rate: ${data.successRate}%`,
                  `Tier: ${data.tier.charAt(0).toUpperCase() + data.tier.slice(1)}`
                ];
              }
            }
          }
        },
        scales: {
          x: {
            display: true,
            title: {
              display: true,
              text: 'Number of Users',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              color: 'rgba(0, 0, 0, 0.1)'
            },
            beginAtZero: true
          },
          y: {
            display: true,
            title: {
              display: true,
              text: 'Number of Documents',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              color: 'rgba(0, 0, 0, 0.1)'
            },
            beginAtZero: true
          }
        }
      }
    };

    this.organizationChart = new Chart(ctx, configuration);
  }

  private updateCharts(): void {
    // Update send type chart
    if (this.sendTypeChart) {
      this.sendTypeChart.data.labels = this.segmentationData.sendTypeBreakdown.map(item => item.name);
      this.sendTypeChart.data.datasets[0].data = this.segmentationData.sendTypeBreakdown.map(item => item.count);
      this.sendTypeChart.update('none');
    }

    // Update device chart
    if (this.deviceChart) {
      this.deviceChart.data.labels = this.segmentationData.deviceBreakdown.map(item => item.name);
      this.deviceChart.data.datasets[0].data = this.segmentationData.deviceBreakdown.map(item => item.count);
      this.deviceChart.data.datasets[1].data = this.segmentationData.deviceBreakdown.map(item => item.successRate);
      this.deviceChart.update('none');
    }

    // Update organization chart
    if (this.organizationChart) {
      const sortedOrgs = [...this.segmentationData.organizationBreakdown]
        .sort((a, b) => b.documentsCount - a.documentsCount)
        .slice(0, 8);

      this.organizationChart.data.datasets[0].data = sortedOrgs.map(org => ({
        x: org.usersCount,
        y: org.documentsCount,
        organizationName: org.organizationName,
        successRate: org.successRate,
        tier: org.tier
      }));
      this.organizationChart.update('none');
    }
  }

  private getOrganizationColor(tier: string): string {
    switch (tier) {
      case 'enterprise': return '#007bff';
      case 'business': return '#28a745';
      case 'standard': return '#ffc107';
      default: return '#6c757d';
    }
  }

  // Public methods for component interactions
  public refreshCharts(): void {
    this.updateCharts();
  }

  public downloadChart(chartType: 'sendType' | 'device' | 'organization'): void {
    let chart: Chart;
    let filename: string;

    switch (chartType) {
      case 'sendType':
        chart = this.sendTypeChart;
        filename = 'wesign-send-type-breakdown';
        break;
      case 'device':
        chart = this.deviceChart;
        filename = 'wesign-device-breakdown';
        break;
      case 'organization':
        chart = this.organizationChart;
        filename = 'wesign-organization-analysis';
        break;
      default:
        return;
    }

    if (chart) {
      const link = document.createElement('a');
      link.download = `${filename}-${Date.now()}.png`;
      link.href = chart.toBase64Image();
      link.click();
    }
  }

  // Utility methods for template
  public getTopOrganizations(limit: number = 5): OrganizationSegment[] {
    return [...this.segmentationData.organizationBreakdown]
      .sort((a, b) => b.documentsCount - a.documentsCount)
      .slice(0, limit);
  }

  public getTopTemplates(limit: number = 5): TemplateUsageData[] {
    return [...this.segmentationData.templateUsage]
      .sort((a, b) => b.usageCount - a.usageCount)
      .slice(0, limit);
  }

  public formatTierBadgeClass(tier: string): string {
    switch (tier) {
      case 'enterprise': return 'badge-primary';
      case 'business': return 'badge-success';
      case 'standard': return 'badge-warning';
      default: return 'badge-secondary';
    }
  }

  public formatPerformanceClass(rate: number): string {
    if (rate >= 90) return 'text-success';
    if (rate >= 75) return 'text-warning';
    return 'text-danger';
  }
}