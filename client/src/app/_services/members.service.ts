import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, model, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  private baseUrl = environment.apiUrl;
  private membersResponseMap = new Map();
  private user = this.accountService.currentUser();

  userParams = signal<UserParams>(new UserParams(this.user));
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);

  resetUser() {
    this.userParams.set(new UserParams(this.user));
  }

  getMembers() {
    var memberKey = Object.values(this.userParams()).join('-');

    var response = this.membersResponseMap.get(memberKey);

    if (response) return this.setResponseToPaginatedResult(response);

    let params = this.setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);

    params = params.append("minAge", this.userParams().minAge);
    params = params.append("maxAge", this.userParams().maxAge);
    params = params.append("gender", this.userParams().gender);
    params = params.append("orderBy", this.userParams().orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', {observe: "response", params}).subscribe({
      next: response => {
        this.setResponseToPaginatedResult(response);
        this.membersResponseMap.set(memberKey, response);
      }
    });
  }

  getMember(username: string) {
    const member: Member = [...this.membersResponseMap.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find((m: Member) => m.username === username);

      console.log(member);

    if (member !== undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      // tap(() => {
      //   this.members.update((members) =>
      //     members.map((m) => (m.username === member.username ? member : m))
      //   );
      // })
    );
  }

  setMainPhoto(photo: Photo) {
    return this.http
      .put(this.baseUrl + 'users/set-main-photo/' + photo.id, {})
      .pipe(
        // tap(() => {
        //   this.members.update((members) =>
        //     members.map((m) => {
        //       if (m.photos.includes(photo)) {
        //         m.photoUrl = photo.url;
        //       }

        //       return m;
        //     })
        //   );
        // })
      );
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      // tap(() => {
      //   this.members.update((members) =>
      //     members.map((m) => {
      //       if (m.photos.includes(photo)) {
      //         m.photos = m.photos.filter(x => x.id !== photo.id);
      //       }

      //       return m;
      //     })
      //   );
      // })
    );
  }

  private setResponseToPaginatedResult(response: HttpResponse<Member[]>) {
    this.paginatedResult.set({
      items: response.body as Member[],
      pagination: JSON.parse(response.headers.get("Pagination")!)
    })
  }

  private setPaginationHeaders(pageNumber: number | undefined, pageSize: number | undefined) {
    let params = new HttpParams();

    if (pageNumber && pageSize) {
      params = params.append("pageNumber", pageNumber);
      params = params.append("pageSize", pageSize);
    }
    return params;
  }
}
