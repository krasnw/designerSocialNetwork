<script>
import ProfileLike from '@/assets/Icons/ProfileLike.vue';
import { imageDirectory, formatNumber } from '@/services/constants';

export default {
  name: "UserBadge",
  components: {
    ProfileLike
  },
  props: {
    data: {
      type: Object,
      required: true
    }
  },
  methods: {
    imagePathHandler(image) {
      return image ? imageDirectory + image : require('@/assets/Images/avatar.png');
    },
    goToProfile() {
      this.$router.push(`/${this.data.user.username}/portfolio`);
    },
    formatNumber
  }
};

</script>

<template>
  <section class="user-badge background" @click="goToProfile">
    <span class="user-badge__info">
      <p class="user-badge__place">{{ data.place }}</p>
      <img class="user-badge__avatar" :src="imagePathHandler(data.user.profileImage)" alt="User avatar">
      <h3 class="user-badge__name">{{ data.user.firstName + " " + data.user.lastName }}</h3>
    </span>
    <span class="user-badge__stats">
      <p>{{ formatNumber(data.likes) }}</p>
      <ProfileLike />
    </span>
  </section>
</template>

<style scoped>
.user-badge {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 20px;
  cursor: pointer;
}

.user-badge__info {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 10px;
}

.user-badge__place {
  font-size: 14px;
  font-weight: 600;
  width: 20px;
}

.user-badge__avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
}

.user-badge__stats {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 20px;
}
</style>