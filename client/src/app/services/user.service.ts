import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseUrl = 'https://localhost:5001/api';
  private usersSource = new BehaviorSubject<User[] | null>(null);

  users$ = this.usersSource.asObservable();

  constructor(private httpClient: HttpClient) { }

  getUsers() {
    return this.httpClient
      .get<User[]>(`${this.baseUrl}/users`)
      .pipe(
        map(users => {
          console.log(users)
          this.usersSource.next(users)
        })
      );
  }
}
