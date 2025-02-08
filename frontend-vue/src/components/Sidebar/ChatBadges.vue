<script>
import Lupa from '@/assets/Icons/Lupa.vue';
import ProfileBadge from './ProfileBadge.vue';
import { chatService } from '@/services/chat';

export default {
  name: "ChatBadges",
  components: {
    ProfileBadge,
    Lupa
  },
  async mounted() {
    this.users = await chatService.getUserChats().catch((error) => {
      this.$router.push(`/error/${error.status}`);
    });
  },
  data() {
    return {
      lookingFor: '',
      pinnedChat: null,
      users: [],
    };
  },
  computed: {
    filteredUsers() {
      const searchTerm = this.lookingFor.toLowerCase();

      const pinnedUser = this.users.find(user => user.username === this.pinnedChat);
      const unpinnedUsers = this.users.filter(user => user.username !== this.pinnedChat);

      const filteredUnpinned = searchTerm
        ? unpinnedUsers.filter(user =>
          user.firstName.toLowerCase().includes(searchTerm) ||
          user.lastName.toLowerCase().includes(searchTerm))
        : unpinnedUsers;

      return pinnedUser ? [pinnedUser, ...filteredUnpinned] : filteredUnpinned;
    }
  },
  created() {
    const username = this.$route.params.username;
    if (username) {
      this.pinnedChat = username;
    }
  },
  watch: {
    '$route.params.username': {
      handler(username) {
        this.pinnedChat = username || null;
      },
      immediate: true
    }
  },
  methods: {
    togglePin(user) {
      const username = user.username;
      const isSameChat = this.pinnedChat === username;

      if (isSameChat) {
        this.pinnedChat = null;
        this.$router.push({ name: 'conversations' });
      } else {
        this.pinnedChat = username;
        const routeConfig = {
          name: 'chat',
          params: { username }
        };

        if (user.chatStatus === 'disabled') {
          routeConfig.query = { isDisabled: true };
        }

        this.$router.push(routeConfig);
      }

      this.lookingFor = '';
    }
  }
};
</script>

<template>
  <article class="profile-badges">
    <div class="search-container">
      <input type="text" class="search" placeholder="Szukaj" v-model="lookingFor" />
      <Lupa class="search-icon" />
    </div>
    <h3>Czaty</h3>
    <h3 v-if="filteredUsers.length === 0" class="info-message">Brak czatów</h3>
    <TransitionGroup name="chat-list">
      <ProfileBadge v-for="user in filteredUsers" :key="user.username" :user="user"
        :isPinned="pinnedChat === user.username" @click="togglePin(user)" />
    </TransitionGroup>
  </article>
</template>

<style scoped>
.info-message {
  text-align: center;
  font-weight: 700;
  font-size: 20px;
  opacity: 0.5;
}

.search-container {
  border: 0.5px solid white;
  background-color: var(--element-light-color);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.search-container:focus-within {
  outline: 1px solid white;
}

.search {
  margin: 0;
  padding: 10px 20px;
  width: 100%;
  box-shadow: none;
  border: none;
  background-color: transparent;
  font-weight: 600;
  font-size: 14px;
}

.search:focus {
  outline: none;
}

.search-icon {
  margin: 0 21px;
}

/* Chat list animations */
.chat-list-move {
  transition: transform 0.5s ease;
}

.chat-list-enter-active,
.chat-list-leave-active {
  transition: all 0.5s ease;
}

.chat-list-enter-from,
.chat-list-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

/* Add gap between chat badges */
.profile-badges :deep(.profile-badge) {
  margin-bottom: 10px;
}

/* Pin animation */
.profile-badges :deep(.profile-badge--pinned) {
  transition: all 0.3s ease;
  transform: scale(1.02);
}
</style>