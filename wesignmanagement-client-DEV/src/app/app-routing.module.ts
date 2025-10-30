import { ChangePasswordComponent } from './components/dashboard/configuration/change-password/change-password.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { ManagementGuard } from './ManagementGuard';


const routes: Routes = [
  {
    path: "",
    pathMatch: "full",
    redirectTo: "/login",
  },
  {      
      component: LoginComponent,
      path: "login",
  },
  {
    canActivate:[ManagementGuard],
    component: DashboardComponent,
    path: "dashboard",
  },
  {
    canActivate:[ManagementGuard],
    component: DashboardComponent,
    path: "dashboard/:view",
  },
  {
    canActivate:[ManagementGuard],
    component: ChangePasswordComponent,
    path: "dashboard/configuration/password",
  },
  { path: "**", redirectTo: "/login", pathMatch: "full" },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
