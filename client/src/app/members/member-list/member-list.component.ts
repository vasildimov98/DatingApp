import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { PageChangedEvent, PaginationModule } from 'ngx-bootstrap/pagination';
import { MemberCardComponent } from '../member-card/member-card.component';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [MemberCardComponent, PaginationModule, FormsModule, ButtonsModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css',
})
export class MemberListComponent implements OnInit {
  memberService = inject(MembersService);

  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'Females' },
  ];

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers();
  }

  resetFilters() {
    this.memberService.resetUserParams();
    this.loadMembers();
  }

  pageChanged($event: PageChangedEvent) {
    if (this.memberService.userParams().pageNumber !== $event.page) {
      this.memberService.userParams().pageNumber = $event.page;
      this.loadMembers();
    }
  }
}
