<script>
import UserBadge from './userBadge.vue';
import Spinner from '@/assets/Icons/Spinner.vue';
import { rankingService, ITEMS_PER_PAGE } from '@/services/ranking';

export default {
  name: "RankingPage",
  components: {
    UserBadge,
    Spinner,
  },
  data() {
    return {
      users: [],
      isLoading: false,
      offset: 0,
      hasMore: true,
      observer: null,
    };
  },
  methods: {
    async loadMore() {
      if (this.isLoading || !this.hasMore) return;

      this.isLoading = true;
      const newUsers = await rankingService.getRanking(this.offset);
      this.isLoading = false;

      if (newUsers.length < ITEMS_PER_PAGE) {
        this.hasMore = false;
      }

      this.users = [...this.users, ...newUsers];
      this.offset += ITEMS_PER_PAGE;
    },
    observeLastElement(entries) {
      if (entries[0].isIntersecting) {
        this.loadMore();
      }
    }
  },
  mounted() {
    this.loadMore();

    this.observer = new IntersectionObserver(this.observeLastElement, {
      threshold: 0.5
    });
  },
  updated() {
    const sentinel = this.$refs.sentinel;
    if (sentinel) {
      this.observer.disconnect();
      this.observer.observe(sentinel);
    }
  },
  beforeUnmount() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }
};
</script>

<template>
  <main>
    <h2 class="page-name">Lider board</h2>
    <section class="ranking">
      <UserBadge v-for="user in users" :key="user.user.username" :data="user" />
      <div v-if="hasMore" ref="sentinel" class="sentinel">
        <Spinner v-if="isLoading" class="spinner" />
      </div>
    </section>
  </main>
</template>

<style scoped>
.ranking {
  display: flex;
  flex-direction: column;
  gap: 10px;
  width: 500px;
  max-width: 750px;
}

.sentinel {
  height: 50px;
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>