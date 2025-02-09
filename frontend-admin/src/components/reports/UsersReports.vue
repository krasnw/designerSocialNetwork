<script setup>
import { ref, computed } from 'vue';
import userReportService from '../../services/users';
import UserBadge from '../views/UserBadge.vue';
import UserView from '../views/UserView.vue';

const selectedOption = ref('');
const error = ref('');
const isLoading = ref(false);
const frozenUsers = ref([]);
const reportedUsers = ref([]);
const searchQuery = ref('');

const options = [
  { value: 'frozen', label: '🥶 Lista zamrożonych' },
  { value: 'reports', label: '📨 Lista reklamacji' },
];

const filteredFrozenUsers = computed(() => {
  return frozenUsers.value.filter(user =>
    user.username.toLowerCase().includes(searchQuery.value.toLowerCase())
  );
});

const changeSelectedOption = async (option) => {
  selectedOption.value = option;
  if (option === 'frozen') {
    await loadFrozenUsersList();
  } else if (option === 'reports') {
    await loadReportedUsersList();
  }
};

const unFreezeUser = async (username) => {
  await userReportService.toggleFrozenUser(username);
}

const loadFrozenUsersList = async () => {
  isLoading.value = true;
  try {
    frozenUsers.value = await userReportService.getFrozenUsers();
  } catch (err) {
    error.value = err.message;
  } finally {
    isLoading.value = false;
  }
};

const loadReportedUsersList = async () => {
  isLoading.value = true;
  try {
    reportedUsers.value = await userReportService.fetchUserReport();
  } catch (err) {
    error.value = err.message;
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="flex items-center justify-center gap-4 py-1">
    <button v-for="option in options" :key="option.value" @click="changeSelectedOption(option.value)"
      class="px-4 py-2 rounded-lg" :class="{ 'bg-blue-500 text-white': selectedOption === option.value }">
      {{ option.label }}
    </button>
  </div>

  <div v-if="error" class="flex items-center justify-center bg-red-400 py-4">
    <p class="w-64 truncate">{{ error }}</p>
  </div>

  <section v-if="selectedOption === 'frozen'">
    <div v-if="isLoading" class="flex items-center justify-center py-4">
      <p class="px-4 py-2 bg-neutral-950 rounded-lg">Ładowanie...</p>
    </div>
    <div v-else class="flex flex-col items-center gap-4">
      <input type="text" name="user-search" id="user-search" class="px-4 py-2 mt-1 bg-neutral-700 rounded-lg"
        placeholder="Wyszukaj użytkownika" v-model="searchQuery">
      <span class="flex gap-2" v-for="user in filteredFrozenUsers" :key="user.username">
        <UserBadge :user="user" />
        <button class="px-4 py-2 bg-amber-500 max-w-max max-h-max self-center"
          @click="unFreezeUser(user.username)">Przewróć</button>
      </span>
    </div>
  </section>

  <section v-else-if="selectedOption === 'reports'">
    <div v-if="isLoading" class="flex items-center justify-center py-4">
      <p class="px-4 py-2 bg-neutral-950 rounded-lg">Ładowanie...</p>
    </div>
    <div v-else class="flex flex-col items-center gap-4">
      <span v-for="userReport in reportedUsers" :key="userReport.id">
        <UserView :userReport="userReport" />
      </span>
    </div>
  </section>
</template>