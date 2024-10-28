import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../_models/member';
import { RouterLink } from '@angular/router';
import { LikesService } from '../../_services/likes.service';
import { PresenceService } from '../../_services/presence.service';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);

  member = input.required<Member>();
  hasLike = computed(() => this.likesService.likeIds().includes(this.member().id))
  isOnline = computed(() => this.presenceService.onlineUsers().includes(this.member().username));

  toggleLike() {
    this.likesService.toggleLike(this.member().id).subscribe({
      next: () => {
        if (this.hasLike()) {
          this.likesService.likeIds.update(x => x.filter(x => x!== this.member().id));
        } else {
          this.likesService.likeIds.update(x => [...x, this.member().id]);
        }
      }
    })
  }
}
