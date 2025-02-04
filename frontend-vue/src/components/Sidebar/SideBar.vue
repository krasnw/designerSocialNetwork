<script>
import SideBarArrow from '@/assets/Icons/SideBarArrow.vue';
import ProfileInfo from './ProfileInfo.vue';
import Filter from './Filter.vue';
import UserInfoAndStats from './UserInfoAndStats.vue';
import ChatBadges from './ChatBadges.vue';

export default {
  name: "SideBar",
  components: {
    SideBarArrow,
    ProfileInfo,
    UserInfoAndStats,
    ChatBadges,
    Filter,
  },
  props: {
    page: {
      type: String,
      required: true
    }
  },
  data() {
    const sidebarAlwaysOpen = localStorage.getItem('sidebarAlwaysOpen') === 'true';
    return {
      hidden: sidebarAlwaysOpen ? false : (JSON.parse(localStorage.getItem('sidebarHidden')) || false),
      rotated: sidebarAlwaysOpen ? true : (JSON.parse(localStorage.getItem('sidebarRotated')) || true),
      alwaysOpen: sidebarAlwaysOpen,
      pagesWithUserInfo: ['myProfile', 'editProfile', 'portfolio', 'addTask', 'subscription'],
    };
  },
  created() {
    window.addEventListener('sidebarSettingChanged', this.handleSidebarSetting);
  },
  beforeUnmount() {
    window.removeEventListener('sidebarSettingChanged', this.handleSidebarSetting);
  },
  watch: {
    hidden(newValue) {
      localStorage.setItem('sidebarHidden', JSON.stringify(newValue));
    },
    rotated(newValue) {
      localStorage.setItem('sidebarRotated', JSON.stringify(newValue));
    },
  },
  methods: {
    toggleSidebar() {
      this.hidden = !this.hidden;
      this.rotated = !this.rotated;
      this.$emit('sidebarStateChanged', this.hidden);
    },
    handleSidebarSetting() {
      this.alwaysOpen = localStorage.getItem('sidebarAlwaysOpen') === 'true';
      if (this.alwaysOpen) {
        this.hidden = false;
        this.rotated = true;
        this.$emit('sidebarStateChanged', false);
      }
    }
  },
  mounted() {
    this.$emit('sidebarStateChanged', this.hidden);
  },
};
</script>

<template>
  <aside :style="{ width: hidden ? '10px' : '350px' }" class="sidebar-wrapper">

    <button class="arrow-button" @click="toggleSidebar" v-if="!alwaysOpen">
      <span :class="{ rotated: rotated }">
        <SideBarArrow />
      </span>
    </button>


    <section v-if="!hidden" class="sidebar">
      <UserInfoAndStats v-if="pagesWithUserInfo.includes(page)" :page="page" />
      <ProfileInfo v-else />

      <!-- cases for other pages -->
      <div v-if="page === 'feed'" class="wrap">
        <Filter />
      </div>
      <div v-else-if="page === 'myProfile' || page === 'portfolio'" class="wrap">
        <Filter />
      </div>
      <div v-else-if="page === 'conversations' || page === 'chat'" class="wrap">
        <ChatBadges />
      </div>
    </section>
  </aside>
</template>

<style scoped>
.wrap {
  margin-top: 20px;
  padding: 0px 20px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.sidebar-wrapper {
  position: fixed;
  top: 0;
  right: 0;
  backdrop-filter: blur(64px);
  height: 100svh;
  background-color: var(--element-dark-color);
  border-left: 0.5px solid var(--element-light-color);
  transition: width 0.3s;
}

.arrow-button {
  position: absolute;
  top: 104px;
  left: -25px;
  width: 25px;
  height: 30px;
  background-color: var(--element-dark-color);
  backdrop-filter: blur(64px);
  border-top: 0.5px solid var(--element-light-color);
  border-left: 0.5px solid var(--element-light-color);
  border-bottom: 0.5px solid var(--element-light-color);
  border-right: none;
  border-radius: 20px 0px 0px 20px;
  cursor: pointer;
}

.arrow-button:hover {
  background-color: var(--element-dark-hover-color);

  span {
    transform: rotate(-20deg);
  }
}

.arrow-button span {
  display: inline-block;
  transition: transform 0.5s;
  display: flex;
  justify-content: center;
  align-items: center;
}

.arrow-button span.rotated {
  transform: rotate(-180deg);
}

.sidebar {
  display: flex;
  flex-direction: column;
  height: 100svh;
  width: 100%;
  background-image: url('/src/assets/Images/noise.png');
  max-height: 100svh;
  /* Добавляем максимальную высоту */
  overflow-y: auto;
  overflow-x: hidden;
  /* Добавляем вертикальную прокрутку */

  /* Стилизация скроллбара */
  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: #0000;
    border-left: 0.5px solid var(--element-light-color);
  }

  &::-webkit-scrollbar-thumb {
    background: var(--element-light-color);
    border-radius: 2px;
  }

  &::-webkit-scrollbar-thumb:hover {
    cursor: pointer;
  }
}

.profile-badges {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
</style>