<script>
import { imageDirectory } from '@/services/constants';
import defaultImage from '@/assets/Images/avatar.png';

export default {
  name: "ProfileBadge",
  props: {
    user: {
      type: Object,
      required: true
    },
    isPinned: {
      type: Boolean,
      default: false
    }
  },
  methods: {
    imagePathHandler(image) {
      return image ? `${imageDirectory}${image}` : defaultImage;
    }
  }
};
</script>

<template>
  <article class="profile-badge" :class="{ 'profile-badge--pinned': isPinned }">
    <img :src="imagePathHandler(user.profileImage)" loading="lazy" alt="User avatar" class="profile-badge__avatar" />
    <h3 class="profile-badge__name">{{ user.firstName + ' ' + user.lastName }}</h3>
  </article>
</template>

<style scoped>
.profile-badge {
  display: flex;
  align-items: center;
  border: 0.5px solid white;
  gap: 20px;
  padding: 8px 15px;
  border-radius: 10px;
  background-color: var(--element-light-color);
}

.profile-badge__avatar {
  border: 0.5px solid white;
  background-color: var(--element-border-light-color);
  padding: 2px;
  width: 35px;
  height: 35px;
  border-radius: 50%;
  object-fit: cover;
}

.profile-badge__name {
  font-size: 14px;
  font-weight: 700;
}

.profile-badge--pinned {
  background-color: var(--element-border-light-color);
}
</style>