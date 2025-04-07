<script>
import PenIcon from '@/assets/Icons/PenIcon.vue';
import ProfileLike from '@/assets/Icons/ProfileLike.vue';
import RankIcon from '@/assets/Icons/RankIcon.vue';
import RubyIcon from '@/assets/Icons/RubyIcon.vue';
import WhiteRuby from '@/assets/Icons/WhiteRuby.vue';
import ShareProfileIcon from '@/assets/Icons/ShareProfileIcon.vue';
import TasksIcon from '@/assets/Icons/TasksIcon.vue';
import { userService } from '@/services/user';
import { imageDirectory } from '@/services/constants';
import defaultAvatar from '@/assets/Images/avatar.png';
import PortfolioIcon from '@/assets/Icons/PortfolioIcon.vue';
import ReportIcon from '@/assets/Icons/ReportIcon.vue';

export default {
  name: "UserInfoAndStats",
  components: {
    RankIcon,
    TasksIcon,
    ProfileLike,
    RubyIcon,
    WhiteRuby,
    ShareProfileIcon,
    PenIcon,
    PortfolioIcon,
    ReportIcon
  },
  props: {
    page: {
      type: String,
      required: true
    },
    username: {
      type: String,
      required: false
    }
  },
  data() {
    return {
      isLoggedIn: localStorage.getItem('JWT') !== null,
      user: {
        username: '',
        name: '',
        image: '',
        rank: 0,
        rubies: 0,
        tasks: 0,
        likes: 0,
        description: ''
      },
    }
  },
  async created() {
    try {
      let userData;
      if (this.page === 'myProfile' || this.page === 'editProfile' || this.page === 'buyRubies') {
        userData = await userService.getMyData();
      } else if (this.page === 'portfolio' || this.page === 'addTask' || this.page === 'subscription') {
        const usernameFromRoute = this.$route.params.username;
        userData = await userService.getUserData(usernameFromRoute);
      } else {
        this.$router.push('/error/404');
        return;
      }

      this.user = {
        username: userData.username,
        name: `${userData.firstName} ${userData.lastName}`,
        image: userData.profileImage || '',
        rubies: Number(userData.rubies || 0),
        rank: Number((userData.rating || userData.ratingPosition) || 0),  //TODO remove ratingPosition when backend is updated
        tasks: Number(userData.completedTasks || 0),
        likes: Number(userData.totalLikes || 0),
        description: userData.description || 'User has not added a description yet'
      };
    } catch (error) {
      if (error.message === "404") {
        this.$router.push('/error/404');
        return;
      }
    }
  },
  computed: {
    formatNumber() {
      return (num) => {
        if (num >= 1000000000) {
          return (num / 1000000000).toFixed(1) + ' b';
        }
        if (num >= 1000000) {
          return (num / 1000000).toFixed(1) + ' m';
        }
        if (num >= 1000) {
          return (num / 1000).toFixed(1) + ' k';
        }
        return num.toString();
      }
    }
  },
  methods: {
    copyLink() {
      navigator.clipboard.writeText('http://localhost:8080/' + this.user.username + '/portfolio');
    },
    reportUser() {
      this.$router.push({
        path: '/report',
        query: {
          username: this.user.username
        }
      });
    },
    navigateToRubies() {
      this.$router.push('/profile/rubies');
    },
    editPath() {
      this.$router.push('/profile/me/edit');
    },
    requestModal() {
      this.$router.push('/' + this.user.username + '/add-task');
    },
    imagePathHandler(image) {
      if (image === '') {
        return defaultAvatar;
      }
      return imageDirectory + image;
    },
    formattedText(text) {
      return text
        ? text.replace(/\r?\n/g, '<br>')
        : '';
    }
  }
};
</script>

<template>
  <span class="wrapper">
    <article class="sidebar-profile">
      <div class="username-wrapper">
        <h3 class="username">{{ user.name }}</h3>
      </div>
    </article>
    <article class="profile-statistics">
      <img class="profile-picture no-select" :src="imagePathHandler(user.image)" alt="Profile picture"
        onmousedown='return false;' ondragstart='return false;' />
      <div class="stat-info">
        <h3>Statistics</h3>
        <div class="stats-container">
          <span class="stat-row">
            <span class="stats-icon">
              <RankIcon />
            </span>
            <span class="stat-label">Rank</span>
            <span class="stat-number">{{ formatNumber(user.rank) }}</span>
          </span>
          <span class="stat-row rubies-link"
            v-if="page === 'myProfile' || page === 'editProfile' || page === 'buyRubies'" @click="navigateToRubies">
            <span class="stats-icon">
              <RubyIcon />
            </span>
            <span class="stat-label">Rubies</span>
            <span class="stat-number">{{ formatNumber(user.rubies) }}</span>
          </span>
          <span class="stat-row">
            <span class="stats-icon">
              <TasksIcon />
            </span>
            <span class="stat-label">Tasks</span>
            <span class="stat-number">{{ formatNumber(user.tasks) }}</span>
          </span>
          <span class="stat-row">
            <span class="stats-icon">
              <ProfileLike />
            </span>
            <span class="stat-label">Likes</span>
            <span class="stat-number">{{ formatNumber(user.likes) }}</span>
          </span>
        </div>
      </div>
    </article>
    <article class="profile-description">
      <h3>Description</h3>
      <div class="divider" />
      <p class="text-description" v-html="formattedText(user.description)"></p>
    </article>
    <button class="main-button subscribe" v-if="page === 'subscription'"
      @click="$router.push(`/${user.username}/portfolio`)">
      View portfolio
      <PortfolioIcon />
    </button>
    <button class="main-button subscribe"
      v-else-if="page !== 'myProfile' && page !== 'editProfile' && page !== 'buyRubies'"
      @click="$router.push(`/${user.username}/subscription`)">
      <!-- If user is not subscribed -->
      <span v-if="!user.isSubscribedTo">
        Buy Subscription
      </span>
      <!-- If user is subscribed -->
      <span v-else>
        Manage Subscription
      </span>
      <WhiteRuby />
    </button>
    <span class="profile-buttons">
      <button class="main-button" v-if="page === 'myProfile' || page === 'buyRubies'" @click="editPath">Edit profile
        <PenIcon />
      </button>
      <button class="main-button" v-else-if="page === 'editProfile'" @click="$router.go(-1)">Back ↩︎
      </button>
      <button class="main-button" v-else @click="requestModal">Request task
        <PenIcon />
      </button>
      <button class="copy-button" @click="copyLink">
        <ShareProfileIcon />
      </button>
      <button class="copy-button" v-if="page !== 'myProfile' && page !== 'buyRubies' && page !== 'editProfile'"
        @click="reportUser">
        <ReportIcon />
      </button>
    </span>
  </span>
</template>

<style scoped>
button {
  font-family: var(--font);
  background-color: var(--element-light-color);
  border-radius: 7px;
  border: 0.5px solid white;
  padding: 10px 20px;
  color: var(--primery-text-color);
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

button:hover {
  background-color: var(--element-hover-light-color);
}

.profile-description {
  margin-top: 20px;
}

.divider {
  width: 100%;
  height: 1px;
  background-color: var(--element-light-color);
  border-radius: 1px;
  margin: 5px 0;
}

.text-description {
  font-size: 13px;
}

.subscribe {
  margin-top: 20px;
}

.main-button {
  display: flex;
  flex-direction: row;
  justify-content: center;
  gap: 10px;
  align-items: center;
  width: 100%;
}

.wrapper {
  padding: 20px 20px;
  background-color: var(--element-light-color);
  border-bottom: 0.5px solid var(--element-border-light-color);
}

.sidebar-profile {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
}

.profile-picture {
  border: 0.5px solid white;
  background-color: var(--element-border-light-color);
  padding: 8px;
  width: 140px;
  height: 140px;
  border-radius: 16px;
  object-fit: cover;
}

.profile-statistics {
  margin-top: 20px;
  display: flex;
  flex-direction: row;
  align-items: stretch;
  gap: 20px;
  width: 100%;
  min-height: 140px;
}

.stat-info {
  flex: 1;
  width: 100%;
  display: flex;
  flex-direction: column;
  justify-content: space-around;
}

.sidebar-profile button {
  background-color: var(--element-light-color);
  border-radius: 7px;
  border: 0.5px solid white;
  padding: 12px 25px;
  color: var(--primery-text-color);
  font-size: 16px;
  font-weight: 700;
}

.sidebar-profile button:hover {
  background-color: var(--element-border-light-color);
  cursor: pointer;
}

.username-wrapper {
  background-color: var(--element-light-color);
  border-radius: 7px;
  width: 100%;
  border: 0.5px solid white;
  padding: 12px 50px;
  cursor: pointer;
  display: flex;
  justify-content: center;
  align-items: center;
}

.username {
  font-size: 16px;
  font-weight: 700;
}


.sidebar-profile img {
  border: 0.5px solid white;
  background-color: var(--element-border-light-color);
  padding: 2px;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
}

.profile-buttons {
  margin-top: 20px;
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  gap: 20px;
}

.stats-container {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;
}

.stat-row {
  display: flex;
  align-items: center;
  width: 100%;
}

.rubies-link {
  cursor: copy;
}

.rubies-link:hover {
  text-decoration: underline;
}

.stats-icon {
  text-align: center;
  width: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-label {
  flex: 1;
  text-align: left;
  font-size: 14px;
  margin-left: 8px;
  color: var(--primery-text-color);
}

.stat-number {
  text-align: right;
  font-size: 13px;
  color: var(--primery-text-color);
}

.stats-icon :deep(svg) {
  transform: scale(0.8);
}
</style>