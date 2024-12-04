<script>
export default {
  name: "ProfileBadge",
  props: {
    user: {
      type: Object,
      required: true
    }
  }
};
</script>

<template>
  <article class="profile-badge">
    <img :src="user.avatar" alt="User avatar" class="profile-badge__avatar" />
    <h3 class="profile-badge__name">{{ user.name }}</h3>
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
</style>