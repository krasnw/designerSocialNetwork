<script>
import AddPostIcon from '@/assets/Icons/AddPostIcon.vue';
import ConversationsIcon from '@/assets/Icons/ConversationsIcon.vue';
import FeedIcon from '@/assets/Icons/FeedIcon.vue';
import RankingIcon from '@/assets/Icons/RankingIcon.vue';
import SettingsIcon from '@/assets/Icons/SettingsIcon.vue';
import TaskListIcon from '@/assets/Icons/TaskListIcon.vue';
import { markRaw } from 'vue'

export default {
  name: "NavigationBar",
  components: {
    AddPostIcon: markRaw(AddPostIcon),
    ConversationsIcon: markRaw(ConversationsIcon),
    FeedIcon: markRaw(FeedIcon),
    RankingIcon: markRaw(RankingIcon),
    SettingsIcon: markRaw(SettingsIcon),
    TaskListIcon: markRaw(TaskListIcon),
  },
  data() {
    return {
      navigationItems: [
        { name: 'feed', component: FeedIcon, weight: 1, titleName: 'Feed' },
        { name: 'addPost', component: AddPostIcon, weight: 2, titleName: 'Add new post' },
        { name: 'conversations', component: ConversationsIcon, weight: 3, titleName: 'Conversations' },
        { name: 'ranking', component: RankingIcon, weight: 4, titleName: 'Ranking page' },
        { name: 'taskList', component: TaskListIcon, weight: 5, titleName: 'List with tasks' }
      ]
    }
  },

  computed: {
    sortedNavigationItems() {
      return [...this.navigationItems].sort((a, b) => {
        if (a.name === this.currentRoute) return -1;
        if (b.name === this.currentRoute) return 1;
        return a.weight - b.weight;
      });
    },
    currentRoute() {
      return this.$route.name === 'chat' ? 'conversations' : this.$route.name;
    }
  },

  methods: {
    handleNavClick(routeName) {
      this.$router.push({ name: routeName })
    }
  }
};
</script>

<template>
  <section class="nav-wrapper">
    <section class="navigation">
      <div class="empty-space" />

      <nav class="main-section">
        <transition-group name="nav-transition">
          <template v-for="item in sortedNavigationItems" :key="item.name">
            <component :is="item.component" :class="{ active: currentRoute === item.name }"
              @click="handleNavClick(item.name)" class="nav-item" />
            <span v-if="currentRoute === item.name && currentRoute !== 'settings'" class="divider" aria-hidden="true"
              role="separator" :key="`divider-${item.name}`"></span>
          </template>
        </transition-group>
      </nav>

      <article>
        <SettingsIcon @click="handleNavClick('settings')" class="nav-item" />
      </article>
    </section>
  </section>
</template>

<style scoped>
.nav-wrapper {
  position: fixed;
  top: 0;
  left: 0;
  backdrop-filter: blur(64px);
  background-color: var(--element-dark-color);
  z-index: 1;
}

.navigation {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 100svh;
  padding: 12px;
  background-image: url('/src/assets/Images/noise.png');
  border-right: 0.5px solid var(--element-light-color);
}

.main-section {
  display: flex;
  flex-direction: column;
  justify-content: space-around;
  width: 100%;
  gap: 12px;
}

.empty-space {
  height: 35px;
  content: '';
}

.divider {
  height: 2px;
  width: 35px;
  margin: 0;
  border: none;
  background-color: var(--element-light-color);
  border-radius: 1px;
}

.nav-item {
  cursor: pointer;
}

.nav-transition-move {
  transition: transform 0.4s ease;
}

.nav-transition-enter-active,
.nav-transition-leave-active {
  transition: all 0.4s ease;
}

.nav-transition-enter-from,
.nav-transition-leave-to {
  opacity: 0;
  transform: translateY(30px);
}

.nav-transition-leave-active {
  position: absolute;
}
</style>