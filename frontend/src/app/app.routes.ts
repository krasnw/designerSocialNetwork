import { devOnlyGuardedExpression } from '@angular/compiler';
import { Routes } from '@angular/router';
import { asapScheduler, takeUntil } from 'rxjs';

import { FeedComponent } from './feed/feed.component';
import { ProfileComponent } from './profile/profile.component';
import { RankingComponent } from './ranking/ranking.component';
import { TaskListComponent } from './task-list/task-list.component';
import { ConversationsComponent } from './conversations/conversations.component';
import { UploadPostComponent } from './upload-post/upload-post.component';
import { SettingsComponent } from './settings/settings.component';


export const routes: Routes = [
  {
    path: 'feed',
    component: FeedComponent,
  },
  {
    path: 'profile',
    component: ProfileComponent,
  },
  {
    path: 'ranking',
    component: RankingComponent,
  },
  {
    path: 'task-list',
    component: TaskListComponent,
  },
  {
    path: 'conversations',
    component: ConversationsComponent,
  },
  {
    path: 'settings',
    component: SettingsComponent,
  }
];