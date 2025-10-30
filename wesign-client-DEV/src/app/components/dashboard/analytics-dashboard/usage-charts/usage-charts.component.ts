import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { UsageAnalytics, TimeSeriesPoint } from '@models/analytics/analytics-models';

// Register Chart.js components
Chart.register(...registerables);

@Component({
  selector: 'sgn-usage-charts',
  templateUrl: './usage-charts.component.html',
  styleUrls: ['./usage-charts.component.scss']
})
export class UsageChartsComponent implements OnInit, OnDestroy, AfterViewInit {
  @Input() usageData: UsageAnalytics = new UsageAnalytics();
  @Input() timeRange: string = '30d';

  @ViewChild('documentsChart', { static: false }) documentsChartRef: ElementRef<HTMLCanvasElement>;
  @ViewChild('usersChart', { static: false }) usersChartRef: ElementRef<HTMLCanvasElement>;

  private documentsChart: Chart;
  private usersChart: Chart;

  // Chart configurations
  private chartColors = {
    created: '#007bff',
    sent: '#28a745',
    signed: '#17a2b8',
    users: '#ffc107',
    background: {
      created: 'rgba(0, 123, 255, 0.1)',
      sent: 'rgba(40, 167, 69, 0.1)',
      signed: 'rgba(23, 162, 184, 0.1)',
      users: 'rgba(255, 193, 7, 0.1)'
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
    if (this.documentsChart) {
      this.documentsChart.destroy();
    }
    if (this.usersChart) {
      this.usersChart.destroy();
    }
  }

  ngOnChanges(): void {
    if (this.documentsChart && this.usersChart) {
      this.updateCharts();
    }
  }

  private initializeCharts(): void {
    if (this.documentsChartRef && this.usersChartRef) {
      this.createDocumentsChart();
      this.createUsersChart();
    }
  }

  private createDocumentsChart(): void {
    const ctx = this.documentsChartRef.nativeElement.getContext('2d');

    const configuration: ChartConfiguration = {
      type: 'line',
      data: {
        labels: this.getTimeLabels(),
        datasets: [
          {
            label: 'Documents Created',
            data: this.usageData.documentCreatedSeries.map(point => point.value),
            borderColor: this.chartColors.created,
            backgroundColor: this.chartColors.background.created,
            borderWidth: 2,
            fill: true,
            tension: 0.4,
            pointRadius: 4,
            pointHoverRadius: 6
          },
          {
            label: 'Documents Sent',
            data: this.usageData.documentSentSeries.map(point => point.value),
            borderColor: this.chartColors.sent,
            backgroundColor: this.chartColors.background.sent,
            borderWidth: 2,
            fill: true,
            tension: 0.4,
            pointRadius: 4,
            pointHoverRadius: 6
          },
          {
            label: 'Documents Signed',
            data: this.usageData.documentSignedSeries.map(point => point.value),
            borderColor: this.chartColors.signed,
            backgroundColor: this.chartColors.background.signed,
            borderWidth: 2,
            fill: true,
            tension: 0.4,
            pointRadius: 4,
            pointHoverRadius: 6
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          title: {
            display: true,
            text: 'Document Processing Trends',
            font: {
              size: 16,
              weight: 'bold'
            },
            color: '#2c3e50'
          },
          legend: {
            position: 'top',
            labels: {
              usePointStyle: true,
              font: {
                size: 12
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
            displayColors: true,
            callbacks: {
              title: (tooltipItems) => {
                return this.formatTooltipDate(tooltipItems[0].label);
              },
              label: (context) => {
                return `${context.dataset.label}: ${context.parsed.y.toLocaleString()}`;
              }
            }
          }
        },
        scales: {
          x: {
            display: true,
            title: {
              display: true,
              text: this.getXAxisLabel(),
              font: {
                weight: 'bold'
              }
            },
            grid: {
              display: true,
              color: 'rgba(0, 0, 0, 0.1)'
            },
            ticks: {
              maxTicksLimit: 10,
              callback: (value, index) => {
                return this.formatXAxisLabel(this.getTimeLabels()[index]);
              }
            }
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
              display: true,
              color: 'rgba(0, 0, 0, 0.1)'
            },
            beginAtZero: true,
            ticks: {
              callback: (value) => {
                return this.formatNumber(value as number);
              }
            }
          }
        },
        interaction: {
          mode: 'nearest',
          axis: 'x',
          intersect: false
        },
        elements: {
          point: {
            hoverBackgroundColor: '#fff',
            hoverBorderWidth: 2
          }
        }
      }
    };

    this.documentsChart = new Chart(ctx, configuration);
  }

  private createUsersChart(): void {
    const ctx = this.usersChartRef.nativeElement.getContext('2d');

    const configuration: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: this.getTimeLabels(),
        datasets: [
          {
            label: 'Active Users',
            data: this.usageData.userActivitySeries.map(point => point.value),
            backgroundColor: this.chartColors.background.users,
            borderColor: this.chartColors.users,
            borderWidth: 2,
            borderRadius: 6,
            borderSkipped: false
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          title: {
            display: true,
            text: 'Daily Active Users',
            font: {
              size: 16,
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
                return this.formatTooltipDate(tooltipItems[0].label);
              },
              label: (context) => {
                return `Active Users: ${context.parsed.y.toLocaleString()}`;
              }
            }
          }
        },
        scales: {
          x: {
            display: true,
            title: {
              display: true,
              text: this.getXAxisLabel(),
              font: {
                weight: 'bold'
              }
            },
            grid: {
              display: false
            },
            ticks: {
              maxTicksLimit: 10,
              callback: (value, index) => {
                return this.formatXAxisLabel(this.getTimeLabels()[index]);
              }
            }
          },
          y: {
            display: true,
            title: {
              display: true,
              text: 'Number of Users',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              display: true,
              color: 'rgba(0, 0, 0, 0.1)'
            },
            beginAtZero: true,
            ticks: {
              callback: (value) => {
                return this.formatNumber(value as number);
              }
            }
          }
        }
      }
    };

    this.usersChart = new Chart(ctx, configuration);
  }

  private updateCharts(): void {
    // Update documents chart
    if (this.documentsChart) {
      this.documentsChart.data.labels = this.getTimeLabels();
      this.documentsChart.data.datasets[0].data = this.usageData.documentCreatedSeries.map(point => point.value);
      this.documentsChart.data.datasets[1].data = this.usageData.documentSentSeries.map(point => point.value);
      this.documentsChart.data.datasets[2].data = this.usageData.documentSignedSeries.map(point => point.value);
      this.documentsChart.update('none');
    }

    // Update users chart
    if (this.usersChart) {
      this.usersChart.data.labels = this.getTimeLabels();
      this.usersChart.data.datasets[0].data = this.usageData.userActivitySeries.map(point => point.value);
      this.usersChart.update('none');
    }
  }

  private getTimeLabels(): string[] {
    if (this.usageData.documentCreatedSeries.length === 0) {
      return [];
    }
    return this.usageData.documentCreatedSeries.map(point =>
      point.timestamp.toLocaleDateString()
    );
  }

  private getXAxisLabel(): string {
    switch (this.timeRange) {
      case '24h': return 'Hours';
      case '7d': return 'Days';
      case '30d': return 'Days';
      case '90d': return 'Weeks';
      default: return 'Time Period';
    }
  }

  private formatXAxisLabel(label: string): string {
    const date = new Date(label);
    switch (this.timeRange) {
      case '24h':
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      case '7d':
        return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
      case '30d':
        return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
      case '90d':
        return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
      default:
        return date.toLocaleDateString();
    }
  }

  private formatTooltipDate(label: string): string {
    const date = new Date(label);
    return date.toLocaleDateString([], {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  private formatNumber(value: number): string {
    if (value >= 1000000) {
      return `${(value / 1000000).toFixed(1)}M`;
    } else if (value >= 1000) {
      return `${(value / 1000).toFixed(1)}K`;
    }
    return value.toLocaleString();
  }

  // Public methods for chart controls
  public refreshCharts(): void {
    this.updateCharts();
  }

  public downloadChart(chartType: 'documents' | 'users'): void {
    const chart = chartType === 'documents' ? this.documentsChart : this.usersChart;
    if (chart) {
      const link = document.createElement('a');
      link.download = `wesign-${chartType}-chart-${Date.now()}.png`;
      link.href = chart.toBase64Image();
      link.click();
    }
  }
}