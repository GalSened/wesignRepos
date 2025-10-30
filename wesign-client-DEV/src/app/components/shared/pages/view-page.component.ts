import { Component, ElementRef, Input, OnInit, Renderer2, ViewChild, OnDestroy, Output, EventEmitter } from "@angular/core";
import { CheckBoxField, ChoiceField, PageDataResult, RadioField, SignatureField, TextField } from "@models/template-api/page-data-result.model";
import { Store } from "@ngrx/store";
import { DocumentApiService } from "@services/document-api.service";
import { SharedService } from "@services/shared.service";
import { Errors } from '@models/error/errors.model';
import { Subscription } from 'rxjs';
import { TextFieldType } from '@models/enums/text-field-type.enum';

@Component({
    selector: "[viewpage]",
    templateUrl: "view-page.component.html",
})

export class ViewPageComponent implements OnInit, OnDestroy {

    @Input() public collectionId: string;

    @Input() public documentId: string;

    @Input() public pageNumber: number;

    @Input() public displayPageNumber: number;

    @Input()
    public pageData: PageDataResult;

    private pageSubs: Subscription;


    public textFields: TextField[] = [];
    public emailFields: TextField[] = [];
    public phoneFields: TextField[] = [];
    public numberFields: TextField[] = [];
    public dateFields: TextField[] = [];
    public timeFields: TextField[] = [];
    public customFields: TextField[] = [];
    public multilineTextFields: TextField[] = [];
    public checkBoxFields: CheckBoxField[] = [];
    public radioFields: RadioField[] = [];
    public radioGroupsNames: string[] = [];
    public signatureFields: SignatureField[] = [];
    public choiceFields: ChoiceField[] = [];
    @Output() public changePageScroll = new EventEmitter<number>();

    constructor(
        private el: ElementRef,
        private renderer: Renderer2,
        private documentApi: DocumentApiService,
        private sharedService: SharedService,
    ) { }

    public onMouseEnter()
    {
        this.changePageScroll.emit(this.pageNumber);
        
    }

    public ngOnInit() {
        this.displayPageNumber = this.displayPageNumber || this.pageNumber;

        this.renderer.addClass(this.el.nativeElement, "X_PAGE");
        this.renderer.setStyle(this.el.nativeElement, "position", "relative");
        this.fixDataInLocalDataStructure();
        this.renderer.setStyle(this.el.nativeElement, "text-align", `left`);
        this.renderer.setStyle(this.el.nativeElement,
            "background-image", `url(${this.pageData.pageImage})`);
        this.renderer.setStyle(this.el.nativeElement, "width", `${this.pageData.pageWidth}px`);
        this.renderer.setStyle(this.el.nativeElement, "height", `${this.pageData.pageHeight}px`);
        this.sharedService.notifyPageLoaded();
    }

    private fixDataInLocalDataStructure() {
        this.textFields = this.getTextFieldByType(TextFieldType.Text)
        this.textFields = this.textFields.filter(x => x.isHidden == false);
        this.emailFields = this.getTextFieldByType(TextFieldType.Email);
        this.phoneFields = this.getTextFieldByType(TextFieldType.Phone);
        this.multilineTextFields = this.getTextFieldByType(TextFieldType.Multiline);
        this.numberFields = this.getTextFieldByType(TextFieldType.Number);
        this.dateFields = this.getTextFieldByType(TextFieldType.Date);
        this.dateFields = this.sharedService.FormatDateTextToDOMDateFormat(this.dateFields);
        this.timeFields = this.getTextFieldByType(TextFieldType.Time);
        this.customFields = this.getTextFieldByType(TextFieldType.Custom);
        this.pageData.pdfFields.radioGroupFields.forEach(x => {
            x.radioFields.forEach(y => {
                if (x.selectedRadioName == y.name) {
                    y.isDefault = true;
                }
                this.radioFields.push(y);
            }
            );
        });

        this.pageData.pdfFields.checkBoxFields.forEach(x => {
            this.checkBoxFields.push()
        });



    }

    private getTextFieldByType(type: TextFieldType) {
        return this.pageData.pdfFields.textFields.filter(x => x.textFieldType == type).map((tx) => tx as TextField);
    }

    public ngOnDestroy() {
        if (this.pageSubs)
            this.pageSubs.unsubscribe();
    }

}
