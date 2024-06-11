import { Component, OnInit } from '@angular/core';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(public accountService: AccountService) {}

  ngOnInit(): void {
  }

  login() {
    this.accountService.login(this.model)
      .subscribe({
        next: (user) => {
          console.log(user);
        },
        error: (error) => console.log(error),
      });
  }

  logout() {
    this.accountService.logout();
  }
}
