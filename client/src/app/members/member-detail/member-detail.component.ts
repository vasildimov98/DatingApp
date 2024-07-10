import { Component, inject, OnInit } from '@angular/core';
import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute } from '@angular/router';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit {
  private memberService = inject(MembersService);
  private route = inject(ActivatedRoute);

  member?: Member;
  images: GalleryItem[] = [];

  ngOnInit(): void {
    this.loadMember();
  }

  private loadMember() {
    var username = this.route.snapshot.paramMap.get('username');

    if (!username) return;

    this.memberService
      .getMember(username)
      .subscribe({
        next: member => {
          this.member = member;
          this.member.photos.forEach(p => {
              this.images.push(new ImageItem({src: p.url, thumb: p.url}));
          })
        }
      })
  }
}
