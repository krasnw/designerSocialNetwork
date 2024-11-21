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
  },
  data() {
    return {
      replay: false
    };
  },
  methods: {
    replayTask() {
      this.replay = !this.replay;
    }
  }
};
</script>

<template>
  <article class="background task" :class="{ 'task-with-replay': replay }">
    <span class="task-info">
      <img class="profile-picture" :src="userProfilePicture" alt="Profile Picture">
      <h4 class="username">{{ username }}</h4>
    </span>
    <p class="task-description">{{ description }}</p>

    <textarea v-if="replay" class="replay-textarea" placeholder="Replay to the task"></textarea>

    <span class="control-buttons">
      <button class="delete-button button" v-if="!replay">Remove</button>
      <button class="delete-button button" @click="replay = false" v-else>Cancel</button>

      <button @click="replayTask" class="accept-button button" v-if="!replay">Replay</button>
      <button @click="replayTask" class="accept-button button" v-else>Send</button>
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

.task-with-replay {
  animation: incHeight 0.5s ease forwards;
}

@keyframes incHeight {
  0% {
    height: auto;
  }

  100% {
    height: auto;
  }
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
  margin-top: 16px;
}

.replay-textarea {
  margin-top: 16px;
  width: 100%;
}

.control-buttons {
  margin-top: 16px;
  display: flex;
  justify-content: flex-end;
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