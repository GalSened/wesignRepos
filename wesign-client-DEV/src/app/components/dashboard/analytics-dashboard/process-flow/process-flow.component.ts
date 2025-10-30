import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit, OnChanges } from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { ProcessFlowData, FunnelStage, StuckDocumentInfo } from '@models/analytics/analytics-models';

Chart.register(...registerables);

@Component({
  selector: 'sgn-process-flow',
  templateUrl: './process-flow.component.html',
  styleUrls: ['./process-flow.component.scss']
})
export class ProcessFlowComponent implements OnInit, OnDestroy, AfterViewInit, OnChanges {
  @Input() processFlowData: ProcessFlowData = new ProcessFlowData();
  @Input() timeRange: string = '30d';

  @ViewChild('funnelChart', { static: false }) funnelChartRef: ElementRef<HTMLCanvasElement>;
  @ViewChild('conversionChart', { static: false }) conversionChartRef: ElementRef<HTMLCanvasElement>;

  private funnelChart: Chart;
  private conversionChart: Chart;

  // Chart colors
  private funnelColors = {
    stages: ['#007bff', '#28a745', '#ffc107', '#17a2b8'],
    dropoff: '#dc3545',
    background: {
      stages: ['rgba(0, 123, 255, 0.1)', 'rgba(40, 167, 69, 0.1)', 'rgba(255, 193, 7, 0.1)', 'rgba(23, 162, 184, 0.1)'],
      dropoff: 'rgba(220, 53, 69, 0.1)'
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
    if (this.funnelChart) {
      this.funnelChart.destroy();
    }
    if (this.conversionChart) {
      this.conversionChart.destroy();
    }
  }

  ngOnChanges(): void {
    if (this.funnelChart && this.conversionChart) {
      this.updateCharts();
    }
  }

  private initializeCharts(): void {
    if (this.funnelChartRef && this.conversionChartRef) {
      this.createFunnelChart();
      this.createConversionChart();
    }
  }

  private createFunnelChart(): void {
    const ctx = this.funnelChartRef.nativeElement.getContext('2d');

    const sortedStages = [...this.processFlowData.funnelStages]
      .sort((a, b) => a.stageOrder - b.stageOrder);

    const configuration: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: sortedStages.map(stage => stage.stageName),
        datasets: [
          {
            label: 'Documents in Stage',
            data: sortedStages.map(stage => stage.documentsCount),
            backgroundColor: sortedStages.map((_, index) =>
              this.funnelColors.background.stages[index % this.funnelColors.background.stages.length]
            ),
            borderColor: sortedStages.map((_, index) =>
              this.funnelColors.stages[index % this.funnelColors.stages.length]
            ),
            borderWidth: 2,
            borderRadius: 6,
            borderSkipped: false
          },
          {
            label: 'Drop-off',
            data: sortedStages.map((stage, index) => {
              if (index === sortedStages.length - 1) return 0;
              const currentCount = stage.documentsCount;
              const nextCount = sortedStages[index + 1].documentsCount;
              return currentCount - nextCount;
            }),
            backgroundColor: this.funnelColors.background.dropoff,
            borderColor: this.funnelColors.dropoff,
            borderWidth: 2,
            borderRadius: 6,
            borderSkipped: false
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        indexAxis: 'y',
        plugins: {
          title: {
            display: true,
            text: 'Document Processing Funnel',
            font: {
              size: 16,
              weight: 'bold'
            },
            color: '#2c3e50',
            padding: {
              bottom: 20
            }
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
            callbacks: {
              label: (context) => {
                const stageIndex = context.dataIndex;
                const stage = sortedStages[stageIndex];

                if (context.datasetIndex === 0) {
                  return [
                    `Documents: ${stage.documentsCount.toLocaleString()}`,
                    `Conversion Rate: ${stage.conversionRate}%`,
                    `Avg Time in Stage: ${stage.averageTimeInStage}min`
                  ];
                } else {
                  const dropOff = context.parsed.x;
                  return dropOff > 0 ? `Drop-off: ${dropOff.toLocaleString()}` : '';
                }
              }
            }
          }
        },
        scales: {
          x: {
            display: true,
            title: {
              display: true,
              text: 'Number of Documents',
              font: {
                weight: 'bold'
              }
            },
            stacked: true,
            grid: {
              color: 'rgba(0, 0, 0, 0.1)'
            },
            beginAtZero: true
          },
          y: {
            display: true,
            title: {
              display: true,
              text: 'Process Stage',
              font: {
                weight: 'bold'
              }
            },
            stacked: true,
            grid: {
              display: false
            }
          }
        },
        interaction: {
          mode: 'index',
          intersect: false
        }
      }
    };

    this.funnelChart = new Chart(ctx, configuration);
  }

  private createConversionChart(): void {
    const ctx = this.conversionChartRef.nativeElement.getContext('2d');

    const sortedStages = [...this.processFlowData.funnelStages]
      .sort((a, b) => a.stageOrder - b.stageOrder);

    const configuration: ChartConfiguration = {
      type: 'line',
      data: {
        labels: sortedStages.map(stage => stage.stageName),
        datasets: [
          {
            label: 'Conversion Rate (%)',
            data: sortedStages.map(stage => stage.conversionRate),
            borderColor: '#28a745',
            backgroundColor: 'rgba(40, 167, 69, 0.1)',
            borderWidth: 3,
            fill: true,
            tension: 0.4,
            pointRadius: 6,
            pointHoverRadius: 8,
            pointBackgroundColor: '#28a745',
            pointBorderColor: '#fff',
            pointBorderWidth: 2
          },
          {
            label: 'Drop-off Rate (%)',
            data: sortedStages.map(stage => stage.dropOffRate),
            borderColor: '#dc3545',
            backgroundColor: 'rgba(220, 53, 69, 0.1)',
            borderWidth: 3,
            fill: true,
            tension: 0.4,
            pointRadius: 6,
            pointHoverRadius: 8,
            pointBackgroundColor: '#dc3545',
            pointBorderColor: '#fff',
            pointBorderWidth: 2
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          title: {
            display: true,
            text: 'Stage Conversion & Drop-off Rates',
            font: {
              size: 16,
              weight: 'bold'
            },
            color: '#2c3e50',
            padding: {
              bottom: 20
            }
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
            callbacks: {
              label: (context) => {
                const stage = sortedStages[context.dataIndex];
                const value = context.parsed.y;

                if (context.datasetIndex === 0) {
                  return `Conversion: ${value}% (${stage.documentsCount.toLocaleString()} docs)`;
                } else {
                  return `Drop-off: ${value}%`;
                }
              },
              afterLabel: (context) => {
                const stage = sortedStages[context.dataIndex];
                return `Avg Time: ${stage.averageTimeInStage}min`;
              }
            }
          }
        },
        scales: {
          x: {
            display: true,
            title: {
              display: true,
              text: 'Process Stage',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              color: 'rgba(0, 0, 0, 0.1)'
            }
          },
          y: {
            display: true,
            title: {
              display: true,
              text: 'Percentage (%)',
              font: {
                weight: 'bold'
              }
            },
            grid: {
              color: 'rgba(0, 0, 0, 0.1)'
            },
            min: 0,
            max: 100,
            ticks: {
              callback: (value) => `${value}%`
            }
          }
        },
        interaction: {
          mode: 'index',
          intersect: false
        }
      }
    };

    this.conversionChart = new Chart(ctx, configuration);
  }

  private updateCharts(): void {
    const sortedStages = [...this.processFlowData.funnelStages]
      .sort((a, b) => a.stageOrder - b.stageOrder);

    // Update funnel chart
    if (this.funnelChart) {
      this.funnelChart.data.labels = sortedStages.map(stage => stage.stageName);
      this.funnelChart.data.datasets[0].data = sortedStages.map(stage => stage.documentsCount);
      this.funnelChart.data.datasets[1].data = sortedStages.map((stage, index) => {
        if (index === sortedStages.length - 1) return 0;
        const currentCount = stage.documentsCount;
        const nextCount = sortedStages[index + 1].documentsCount;
        return currentCount - nextCount;
      });
      this.funnelChart.update('none');
    }

    // Update conversion chart
    if (this.conversionChart) {
      this.conversionChart.data.labels = sortedStages.map(stage => stage.stageName);
      this.conversionChart.data.datasets[0].data = sortedStages.map(stage => stage.conversionRate);
      this.conversionChart.data.datasets[1].data = sortedStages.map(stage => stage.dropOffRate);
      this.conversionChart.update('none');
    }
  }

  // Public methods for component interactions
  public refreshCharts(): void {
    this.updateCharts();
  }

  public downloadChart(chartType: 'funnel' | 'conversion'): void {
    const chart = chartType === 'funnel' ? this.funnelChart : this.conversionChart;
    if (chart) {
      const link = document.createElement('a');
      link.download = `wesign-${chartType}-analysis-${Date.now()}.png`;
      link.href = chart.toBase64Image();
      link.click();
    }
  }

  // Utility methods for template
  public getOverallConversionRate(): number {
    if (this.processFlowData.funnelStages.length === 0) return 0;

    const sortedStages = [...this.processFlowData.funnelStages]
      .sort((a, b) => a.stageOrder - b.stageOrder);

    const firstStage = sortedStages[0];
    const lastStage = sortedStages[sortedStages.length - 1];

    return Math.round((lastStage.documentsCount / firstStage.documentsCount) * 100);
  }

  public getBiggestDropOff(): { stage: string, percentage: number } {
    const sortedStages = [...this.processFlowData.funnelStages]
      .sort((a, b) => a.stageOrder - b.stageOrder);

    let maxDropOff = 0;
    let maxDropOffStage = '';

    for (let i = 0; i < sortedStages.length - 1; i++) {
      const currentCount = sortedStages[i].documentsCount;
      const nextCount = sortedStages[i + 1].documentsCount;
      const dropOffPercentage = Math.round(((currentCount - nextCount) / currentCount) * 100);

      if (dropOffPercentage > maxDropOff) {
        maxDropOff = dropOffPercentage;
        maxDropOffStage = `${sortedStages[i].stageName} → ${sortedStages[i + 1].stageName}`;
      }
    }

    return { stage: maxDropOffStage, percentage: maxDropOff };
  }

  public getAverageTimeToComplete(): number {
    const sortedStages = [...this.processFlowData.funnelStages]
      .sort((a, b) => a.stageOrder - b.stageOrder);

    return sortedStages.reduce((total, stage) => total + stage.averageTimeInStage, 0);
  }

  public getStuckDocumentsByPriority(priority: 'high' | 'medium' | 'low'): StuckDocumentInfo[] {
    return this.processFlowData.stuckDocuments.filter(doc => doc.priorityLevel === priority);
  }

  public formatStuckTime(hours: number): string {
    if (hours < 24) {
      return `${hours}h`;
    } else {
      const days = Math.floor(hours / 24);
      const remainingHours = hours % 24;
      return remainingHours > 0 ? `${days}d ${remainingHours}h` : `${days}d`;
    }
  }

  public getStuckDocumentsBadgeClass(priority: string): string {
    switch (priority) {
      case 'high': return 'badge-danger';
      case 'medium': return 'badge-warning';
      case 'low': return 'badge-info';
      default: return 'badge-secondary';
    }
  }

  public getRecoverableBadgeClass(isRecoverable: boolean): string {
    return isRecoverable ? 'badge-success' : 'badge-secondary';
  }
}