import { Component, ElementRef, OnInit, ViewChild } from "@angular/core";
import { ContactFilter } from "@models/contacts/contact-filter.model";
import { Contact, SendingMethod, Seal } from "@models/contacts/contact.model";
import { Modal } from '@models/modal/modal.model';
import { ContactApiService } from "@services/contact-api.service";
import { PagerService } from "@services/pager.service";
import { SharedService } from "@services/shared.service";
import { ModalService } from "@services/modal.service";
import { Observable, Subscription, fromEvent } from "rxjs";
import { map, tap, switchMap, debounceTime, distinctUntilChanged } from "rxjs/operators";
import { TranslateService } from '@ngx-translate/core';
import { Errors } from '@models/error/errors.model';
import { AlertLevel, IAppState } from '@state/app-state.interface';
import { Contacts } from '@models/contacts/contacts.model';
import { BatchRequest } from '@models/document-api/batch-request.model';
import * as fieldsActions from "@state/actions/fields.actions";
import * as documentActions from "@state/actions/document.actions";
import * as selectActions from "@state/actions/selection.actions";
import { Store } from '@ngrx/store';
import { Router } from '@angular/router';

@Component({
    selector: "sgn-contacts-component",
    templateUrl: "contacts.component.html",
})

export class ContactsComponent implements OnInit {
    public selectedMethod: any;
    public contactToDelete: Contact;
    public isBusy = false;
    public reader = new FileReader();
    public showContactEdit: boolean; //number = -1;
    public contacts$: Observable<Contact[]>;
    public contactFilter = new ContactFilter();
    public currentPage = 1;
    public pageCalc: any;
    public newContact = new Contact();
    private PAGE_SIZE = 10;
    private deleteModal: Modal;
    private phoneExt: string = "972";
    private deleteSubscription: Subscription;
    public orderByField = 'name';
    public allSelected = false;
    public orderByDesc = false;
    @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
    public telOptions: any;
    public showSearchSpinner = false;
    SendingMethod: typeof SendingMethod = SendingMethod;
    isEditMode: boolean = false;
    public selectedContacts: string[] = [];
    public deleteAllPopupData = new Modal();
    public deleteContactWithPendingDocsData = new Modal();

    public get SendingMethodOptions() {
        let x = Object.values(SharedService.EnumToArrayHelper<SendingMethod>(SendingMethod)).filter(x => x != "TABLET");
        return x;
    };

    constructor(
        private contactsApi: ContactApiService,
        private pager: PagerService,
        private sharedService: SharedService,
        private modalService: ModalService,
        private translate: TranslateService,
        private store: Store<IAppState>,
        public router: Router) {
    }

    public ngOnInit() {
        this.showContactEdit = false;
        this.store.dispatch(new documentActions.ClearFileUploadRequestAction({}));
        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
        this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}))
        fromEvent(this.searchInput.nativeElement, 'keyup').pipe(
            // get value
            map((event: any) => {
                return event.target.value;
            })
            // if character length greater then 2
            //,filter(res => res.length > 2)
            //Search function will not get called when the field is empty
            //,filter(Boolean)
            // Time in milliseconds between key events
            , debounceTime(1000)
            // If previous query is diffent from current   
            , distinctUntilChanged()
        ).subscribe((text: string) => {
            this.currentPage = 1;
            this.updateData();
        });

        this.updateData();

        this.deleteSubscription =
            this.modalService.checkConfirm()
                .pipe(
                    switchMap(res => { return res; }),
                    switchMap((res: number) => {
                        this.sharedService.setBusy(true, "CONTACTS.DELETING");
                        return this.contactsApi.deleteContact(res)
                    })
                ).subscribe(
                    res => {
                        this.updateData();
                        this.sharedService.setSuccessAlert("CONTACTS.DELETED_SUCCESSFULY");
                        this.sharedService.setBusy(false);
                    }, err => {
                        if (err.error && err.error.status && err.error.status == 152) {
                            this.translate.get(["CONTACTS.CONTACT_HAS_PENDING_DOCUMENTS", "BUTTONS.CLOSE", "GLOBAL.NOTICE"]).subscribe(msg => {
                                let keys = Object.keys(msg);
                                let content = msg[keys[0]].replace("[Contact Name]", this.contactToDelete.name);
                                this.deleteContactWithPendingDocsData.showModal = true;
                                this.deleteContactWithPendingDocsData.content = content;
                                this.deleteContactWithPendingDocsData.confirmBtnText = msg[keys[1]];
                                this.deleteContactWithPendingDocsData.title = msg[keys[2]];
                            })
                        }
                        else {
                            this.sharedService.setErrorAlert(new Errors(err.error));
                            this.sharedService.setBusy(false);
                        }
                    }, () => { this.sharedService.setBusy(false); }
                );

        this.sharedService.getSupportedCountries().subscribe((x) => {
            this.telOptions = x;
        });
        this.searchInput.nativeElement.focus();
    }

    ngOnDestroy() {
        this.deleteSubscription.unsubscribe();
    }

    pageChanged(page: number) {
        this.currentPage = page;
        this.updateData();
    }

    onDeleteContactWithPendingDocsSubmitted() {
        this.deleteContactWithPendingDocsData.showModal = false;
        this.sharedService.setBusy(false);
    }

    deleteContactsAllSelected($event) {
        if (this.selectedContacts.length == 0) {
            return;
        }
        $event.preventDefault();
        $event.stopPropagation();
        this.deleteAllPopupData.showModal = true;
        this.translate.get(['DOCUMENT.MODALTITLE', 'CONTACTS.MODALTEXT_BATCH_DELETE', 'REGISTER.CANCEL', 'GLOBAL.DELETE'])
            .subscribe((res: object) => {
                let keys = Object.keys(res);
                this.deleteAllPopupData.title = res[keys[0]];
                this.deleteAllPopupData.content = (res[keys[1]] as string).replace("{#*#}", this.selectedContacts.length.toString());
                this.deleteAllPopupData.rejectBtnText = res[keys[2]];
                this.deleteAllPopupData.confirmBtnText = res[keys[3]];
            });

    }

    doDeleteAllContactEvent() {
        this.deleteAllPopupData.showModal = false;
        this.sharedService.setBusy(true, "CONTACTS.DELETING");
        let contactBatchReq = new BatchRequest();
        contactBatchReq.Ids = this.selectedContacts;
        return this.contactsApi.deleteContactsBatch(contactBatchReq).subscribe(
            res => {
                this.updateData();
                if (res && res.length > 0) {
                    this.translate.get(["CONTACTS.CONTACT_HAS_PENDING_DOCUMENTS_BATCH", "BUTTONS.CLOSE", "GLOBAL.NOTICE"]).subscribe(msg => {
                                let keys = Object.keys(msg);
                                let content = msg[keys[0]].replace("[List of Contact Names]", res.join(", "));
                                this.deleteContactWithPendingDocsData.showModal = true;
                                this.deleteContactWithPendingDocsData.content = content;
                                this.deleteContactWithPendingDocsData.confirmBtnText = msg[keys[1]];
                                this.deleteContactWithPendingDocsData.title = msg[keys[2]];
                            })
                }
                else {
                    this.sharedService.setSuccessAlert("CONTACTS.DELETEDS_SUCCESSFULY");
                    this.sharedService.setBusy(false);
                }
            }, err => {
                this.sharedService.setErrorAlert(new Errors(err.error));
                this.sharedService.setBusy(false);
            }, () => {
            });
    }

    selectedALL() {

        this.selectedContacts = [];
        if (!this.allSelected) {
            this.contacts$.pipe(
                switchMap(res => { return res; })).subscribe(con => this.selectedContacts.push(con.id));
        }

        this.allSelected = !this.allSelected;


    }

    isContactSelected(docCollecteionID) {
        return this.selectedContacts.findIndex((t) => t == docCollecteionID) > -1;
    }

    selecteContact($event, contactID) {

        if (this.isContactSelected(contactID)) {
            this.selectedContacts = this.selectedContacts.filter(item => item != contactID);
        }

        else {
            this.selectedContacts.push(contactID);
        }
    }

    updateData() {
        this.showSearchSpinner = true;
        this.selectedContacts = [];
        this.allSelected = false;
        this.contactFilter.limit = this.PAGE_SIZE;
        this.contactFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;
        this.contactFilter.includeTabletMode = false;

        this.contacts$ = this.contactsApi.getContacts(this.contactFilter).pipe(
            tap((data) => {
                const total = +data.headers.get("x-total-count");
                if (this.currentPage > total / this.PAGE_SIZE) {

                    this.currentPage = Math.ceil(total / this.PAGE_SIZE) > 0 ? Math.ceil(total / this.PAGE_SIZE) : 1;

                }
                this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
            }),
            map((res) => res.body.contacts),
            tap(_ => {
                this.sharedService.setBusy(false);
                this.showSearchSpinner = false;
            })
        );
    }

    addContact() {
        this.showContactEdit = false;
        this.contactsApi.addContact(this.newContact).subscribe((res) => {
            this.updateData();
            this.sharedService.setSuccessAlert("CONTACTS.SAVED_SUCCESSFULY");
            this.newContact = new Contact();
            this.isEditMode = false;
        }, (error) => {
            this.sharedService.setErrorAlert(new Errors(error.error));
            this.newContact = new Contact();
            this.isEditMode = false;
        });
    }

    deleteContact(contact: Contact) {
        this.deleteModal = new Modal({ showModal: true });

        this.translate.get(['CONTACTS.MODALTITLE', 'CONTACTS.MODALTEXT', 'CONTACTS.MODALCANCELBTN', 'CONTACTS.MODALDELETEBTN']).subscribe((res: object) => {
            let keys = Object.keys(res);
            this.deleteModal.title = res[keys[0]];
            this.deleteModal.content = (res[keys[1]] as string).replace("{#*#}", contact.name);

            this.deleteModal.rejectBtnText = res[keys[2]];
            this.deleteModal.confirmBtnText = res[keys[3]];

            this.contactToDelete = contact;

            let confirmAction = new Observable(ob => {
                ob.next(contact.id);
                ob.complete();

                return { unsubscribe() { } };
            });
            this.deleteModal.confirmAction = confirmAction;

            this.modalService.showModal(this.deleteModal);
        });
    }

    saveContact(contact: Contact) {
        this.sharedService.setBusy(true, "CONTACTS.LOADING");
        this.contactsApi.saveContact(contact).subscribe((res) => {
            this.updateData();
            this.sharedService.setSuccessAlert("CONTACTS.SAVED_SUCCESSFULY");
            this.sharedService.setBusy(false);
            this.newContact = new Contact();
            this.isEditMode = false;
            this.showContactEdit = false;
        }, (error) => {
            this.sharedService.setErrorAlert(new Errors(error.error));
            this.sharedService.setBusy(false);
            this.newContact = new Contact();
            this.isEditMode = false;
            this.showContactEdit = false;
        });
    }

    editContact(contactRowId: number) {
        (document.getElementsByClassName("ng-contact-name-input")[contactRowId] as HTMLElement).focus()
    }

    blurDOM() {
        (document.activeElement as HTMLElement).blur();
    }

    UpdateField(newValue: any, contact: Contact, property: string) {
        if (newValue.valid && newValue.value !== contact[property]) {
            contact[property] = newValue.value;
            this.saveContact(contact);
        }
    }

    restoreVal(elemVal: any) {
        elemVal.reset(elemVal.model);
    }

    dropDownUpdate(newValue, contact: Contact, property: string) {
        if (newValue && newValue !== contact[property]) {
            contact[property] = newValue.target.value;
            this.saveContact(contact);
        }
    }

    sealChange(event: any, contact: Contact, sealId: string = null) {
        let file = event.dataTransfer ? event.dataTransfer.files[0] : event.target.files[0],
            pattern = /image-*/,
            reader = new FileReader(),
            _this = this;

        if (!file.type.match(pattern)) {
            this.sharedService.setTranslateAlert("ERROR.INPUT.I_FILE", AlertLevel.ERROR);
            return;
        }
        reader.readAsDataURL(file);
        reader.onload = function (_event) {
            contact.seals = contact.seals || [];
            let s = new Seal();
            s.name = file.name;
            s.base64Image = reader.result.toString();
            if (sealId) {
                let si = contact.seals.findIndex(a => a.id == sealId);
                if (si > -1) {
                    s.id = contact.seals[si].id;
                    contact.seals[si].base64Image = s.base64Image; // 
                    contact.seals[si].name = s.name; // 
                }
            }
            if (!s.id) {
                contact.seals.push(s);
            }
            _this.saveContact(contact);
        }

    }

    removeSeal(contact: Contact, sealId: string) {
        if (contact && contact.seals && sealId) {
            let si = contact.seals.findIndex(a => a.id == sealId);
            if (si > -1) {
                contact.seals.splice(si, 1).slice(0);
                this.saveContact(contact);
            }
        }
    }

    trackByFn(index, item) {
        //https://angular.io/api/common/NgForOf#description
        //https://netbasal.com/angular-2-improve-performance-with-trackby-cc147b5104e5
        //console.log("item: ", item);
        //console.log(document.getElementsByClassName("ng-contact-name-input")[index]);
        return index; // or item.id
    }

    orderByFunction(prop: string) {
        if (prop) {
            if (this.orderByField == prop) {
                this.orderByDesc = !this.orderByDesc;
            }
            this.orderByField = prop;
        }
    }

    importFile(event) {
        this.isBusy = true;
        if (event.target.files.length > 0) {
            let file: File = event.target.files[0];
            //define reader as global - best practice for no memory leak O_o
            this.reader.readAsDataURL(file);
            this.reader.onload = () => {
                let contacts = new Contacts();
                contacts.base64File = this.reader.result.toString();
                this.contactsApi.addContacts(contacts).subscribe((res) => {
                    this.updateData();

                    const contactsCount = parseInt(res.headers.get("x-total-count"));
                    if (contactsCount > 0) {
                        this.sharedService.setSuccessAlert("CONTACTS.SAVED_SUCCESSFULY");
                    }

                }, (error) => {
                    this.sharedService.setErrorAlert(new Errors(error.error));
                });
            };

        }
        this.isBusy = false;
    }

    onSendingMethodChange(contact: Contact, $event) {
        contact.defaultSendingMethod = $event;
        this.saveContact(contact);
    }

    onCountryChange(obj) {
        this.phoneExt = obj.dialCode;
    }

    openEditForm(contact: Contact) {
        this.showContactEdit = true;
        this.newContact = new Contact();
        this.newContact = contact;
        this.isEditMode = true;
    }

    addOrEditContact() {
        this.updateData();
        this.sharedService.setBusy(false);
        this.newContact = new Contact();
        this.isEditMode = false;
        this.showContactEdit = false;
    }

    closeEditForm() {
        this.sharedService.setBusy(false);
        this.showContactEdit = !this.showContactEdit;
        this.newContact = new Contact();
    }
}