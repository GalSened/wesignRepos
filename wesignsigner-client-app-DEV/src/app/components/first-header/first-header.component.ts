import { Component, OnInit } from '@angular/core';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-header',
  templateUrl: './first-header.component.html',
  styleUrls: ['./first-header.component.scss']
})
export class FirstHeaderComponent implements OnInit {

  public documentCollectionName : string = "";  

  constructor(private stateService: StateService) { }

  ngOnInit(): void {
    this.stateService.state$.subscribe(
      x=>{        
        if(x.documentCollectionData ){
          this.documentCollectionName = x.documentCollectionData.name;

          if (x.companyLogo != null && x.companyLogo != ''){
            let el = (<HTMLDivElement>document.getElementById("logo_image"));
            if (el) {                
                el.style.backgroundImage = `url(${x.companyLogo})`;
                el.style.backgroundRepeat = 'no-repeat';
                el.style.backgroundPosition ='center';
                el.style.backgroundSize = 'contain';
                el.style.height = 'inherit';
            }
          }
        }
      });
  }
}