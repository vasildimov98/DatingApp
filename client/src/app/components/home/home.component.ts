import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  registerMode = false;

  constructor(public userService: UserService) {}

  ngOnInit(): void {
    this.getUsers();
  }

  toggleRegisterMode() {
    this.registerMode = !this.registerMode;
  }

  getUsers() {
    this.userService
        .getUsers()
        .subscribe();
  }

  cancelRegister(value: boolean) {
    this.registerMode = value;
  }
}
