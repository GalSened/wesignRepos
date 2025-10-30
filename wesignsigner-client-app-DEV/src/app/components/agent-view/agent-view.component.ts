import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { AgentService } from 'src/app/services/agent.service';

@Component({
  selector: 'app-agent-view',
  templateUrl: './agent-view.component.html',
  styleUrls: ['./agent-view.component.scss']
})

export class AgentViewComponent implements OnInit {

  url: string = "https://wesign3.comda.co.il";
  urlSafe: SafeResourceUrl;
  id: any;
  companyId: any;
  showAd: boolean = true;
  connectionId: string;

  constructor(public sanitizer: DomSanitizer, private agentService: AgentService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.trustUrl(this.url);
    this.route.paramMap.subscribe(params => {
      this.id = params.get("id");
      this.companyId = params.get("companyId");
      this.connectionId = (this.companyId + "_" + this.id).toLowerCase();
    });
    setTimeout(() => {
      this.agentService.connect(this.connectionId);
      this.agentService.listenToOnLinkChange();
      this.agentService.listenToOnMoveToAd();
      this.agentService.listenToImAlive();
      this.agentService.linkChangeSubject.subscribe(
        (link: string) => {
          this.showAd = false;
          this.trustUrl(link);
        }
      );
      this.agentService.moveToAdSubject.subscribe(
        () => {
          this.showAd = true;
          this.trustUrl("");
        });
      this.agentService.reconnectSubject.subscribe(() => {
        this.agentService.connect(this.connectionId);
        this.agentService.listenToOnLinkChange();
        this.agentService.listenToOnMoveToAd();
        this.agentService.listenToImAlive();
      });
    }, 3000);

    setInterval(() =>
      this.agentService.imAlive(this.connectionId), 30000);
  }

  trustUrl(url) {
    this.urlSafe = this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }
}