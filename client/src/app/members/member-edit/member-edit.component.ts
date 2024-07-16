import {
  Component,
  HostListener,
  inject,
  OnInit,
  ViewChild,
} from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { every } from 'rxjs';

@Component({
  selector: 'app-member-edit',
  standalone: true,
  imports: [TabsModule, FormsModule],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css',
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm?: NgForm;
  @HostListener('window:beforeunload', ['$event']) notify($event: any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
    }
  }

  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  private toastr = inject(ToastrService);

  member?: Member;

  ngOnInit(): void {
    this.loadMember();
  }

  updateMember() {
    this.memberService
        .updateMember(this.editForm?.value)
        .subscribe({
          next: _ => {
            this.toastr.success('Profile updated successfully');
            this.editForm?.reset(this.member);
          }
        });
  }

  private loadMember() {
    const user = this.accountService.currentUser();

    if (!user) return;

    this.memberService.getMember(user.username).subscribe({
      next: (member) => (this.member = member),
    });
  }
}
