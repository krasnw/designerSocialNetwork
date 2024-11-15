<script>
export default {
  name: "TaskView",
  props: {
    username: {
      type: String,
      required: true
    },
    userProfilePicture: {
      type: String,
      required: true
    },
    description: {
      type: String,
      required: true
    }
  }
};
</script>

<template>
  <article class="background task">
    <span class="task-info">
      <img class="profile-picture" :src="userProfilePicture" alt="Profile Picture">
      <h4 class="username">{{ username }}</h4>
    </span>
    <p class="task-description">{{ description }}</p>
    <span class="control-buttons">
      <button class="delete-button button">Remove</button>
      <button class="accept-button button">Complete</button>
    </span>
  </article>
</template>

<style scoped>
.task {
  padding: 30px 40px;
  max-width: 925px;
  width: 80%;
  min-width: 550px;
}

.username {
  font-weight: 700;
  font-size: 21px;
}

.task-info {
  display: flex;
  align-items: center;
  gap: 18px;
}

.profile-picture {
  width: 40px;
  height: 40px;
  border: 0.5px solid white;
  padding: 2px;
  border-radius: 50%;
}

.task-description {
  font-weight: 400;
  font-size: 18px;
}

.control-buttons {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
  gap: 20px;
}

.delete-button {
  background: rgba(255, 57, 57, 0.40);
  border-color: rgba(255, 94, 94, 0.60);
}

.accept-button {
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(255, 255, 255, 0.60);
}
</style>