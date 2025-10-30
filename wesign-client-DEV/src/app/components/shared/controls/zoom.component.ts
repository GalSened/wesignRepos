import { AfterViewInit, Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { DeviceDetectorService } from 'ngx-device-detector';

@Component({
    selector: "sgn-zoom",
    templateUrl: 'zoom.component.html'
})

export class ZoomComponent implements OnInit, AfterViewInit {
    @Input()
    public currentPage: number = 1;
    public isDesktopDevice: boolean;
    public isDark : boolean = true;

    @Input() public totalPages: number = 1;
    @Input() public zmodel: { ZoomLevel: number, Bright: boolean };
    @Output() public zmodelChange = new EventEmitter<{ ZoomLevel: number, Bright: boolean }>();
    @Output() public changeBackgroundEvent = new EventEmitter<string>();

    constructor(private deviceService: DeviceDetectorService) {
        this.zmodel = { ZoomLevel: 1, Bright: true };
    }

    ngOnInit(): void {
        this.isDesktopDevice = this.deviceService.isDesktop();
    }

    ngAfterViewInit(): void {
        setTimeout(x => {
            if (!this.isDesktopDevice) {
                this.onZoomOptionChanged();
            }
        }, 1000)

       
    }

    public zoomIn() {
        if (this.zmodel.ZoomLevel < 1.8) {
            this.zmodel.ZoomLevel += 0.25;
        }
        this.zmodelChange.emit(this.zmodel);
    }

    public zoomOut() {
        if (this.zmodel.ZoomLevel > 0.8) {
            this.zmodel.ZoomLevel -= 0.25;
        }
        this.zmodelChange.emit(this.zmodel);
    }

    public toggleBrightness() {
        this.zmodel.Bright = !this.zmodel.Bright;
        this.zmodelChange.emit(this.zmodel);
    }

    public onZoomOptionChanged() {
        let option = (<HTMLSelectElement>document.getElementById("zoomOptions")).value;
        option = option.replace("%", "");
        this.zmodel.ZoomLevel = parseInt(option) / 100;
        this.zmodelChange.emit(this.zmodel);
    }

    public goToPage(pageNum: number) {
        pageNum = Math.min(Math.max(pageNum, 1), this.totalPages);
        let pageElement = document.getElementsByClassName("doc__image X_PAGE")[pageNum - 1];
        if (pageElement != undefined) {
            pageElement.scrollIntoView(true);
            this.currentPage = pageNum;
        }
    }

    public changeBackground() {
        this.isDark = !this.isDark
        this.changeBackgroundEvent.emit();
    }
}
