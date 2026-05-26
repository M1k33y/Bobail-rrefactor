import { useEffect, useState } from "react";
import { Search } from "lucide-react";
import { useSearchParams } from "react-router-dom";
import { adminApi } from "../api/adminApi";
import { useAuth } from "../../auth/hooks/useAuth";
import "../styles/AdminUsersPage.css";

const PAGE_SIZE = 25;

function formatCreatedAt(value) {
  return new Intl.DateTimeFormat("en-US", {
    dateStyle: "medium",
  }).format(new Date(value));
}

function AdminUsersPage() {
  const { userId } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const [users, setUsers] = useState([]);
  const [pageInfo, setPageInfo] = useState({
    page: 1,
    totalPages: 0,
    totalCount: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [searchInput, setSearchInput] = useState(searchParams.get("search") ?? "");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [actionUserId, setActionUserId] = useState("");

  const pageParam = Number.parseInt(searchParams.get("page") ?? "1", 10);
  const currentPage = Number.isNaN(pageParam) || pageParam < 1 ? 1 : pageParam;
  const search = searchParams.get("search") ?? "";

  useEffect(() => {
    setSearchInput(search);
  }, [search]);

  useEffect(() => {
    let active = true;

    async function loadUsers() {
      try {
        setLoading(true);
        const data = await adminApi.getUsers({
          page: currentPage,
          pageSize: PAGE_SIZE,
          search,
        });

        if (!active) {
          return;
        }

        setUsers(data.items ?? []);
        setPageInfo({
          page: data.page ?? 1,
          totalPages: data.totalPages ?? 0,
          totalCount: data.totalCount ?? 0,
          hasPreviousPage: data.hasPreviousPage ?? false,
          hasNextPage: data.hasNextPage ?? false,
        });
        setError("");

        if ((data.page ?? 1) !== currentPage) {
          const params = {};

          if ((data.page ?? 1) > 1) {
            params.page = String(data.page ?? 1);
          }

          if (search.trim()) {
            params.search = search.trim();
          }

          setSearchParams(params, { replace: true });
        }
      } catch (err) {
        if (active) {
          setError(err.message || "Failed to load users.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    loadUsers();

    return () => {
      active = false;
    };
  }, [currentPage, search, setSearchParams]);

  function updateSearchParams(next, replace = false) {
    const params = {};

    if (next.page > 1) {
      params.page = String(next.page);
    }

    if (next.search?.trim()) {
      params.search = next.search.trim();
    }

    setSearchParams(params, { replace });
  }

  function handleSearchSubmit(event) {
    event.preventDefault();
    updateSearchParams({ page: 1, search: searchInput });
  }

  function goToPage(page) {
    updateSearchParams({ page, search });
  }

  async function handleUserAction(user) {
    const action = user.isActive ? "ban" : "unban";
    const confirmed = window.confirm(`Are you sure you want to ${action} ${user.email}?`);

    if (!confirmed) {
      return;
    }

    const previousUsers = users;

    try {
      setActionUserId(user.id);
      setError("");
      setUsers((currentUsers) =>
        currentUsers.map((candidate) =>
          candidate.id === user.id
            ? { ...candidate, isActive: !candidate.isActive }
            : candidate
        )
      );

      const response = user.isActive
        ? await adminApi.banUser(user.id)
        : await adminApi.unbanUser(user.id);
      const updatedUser = response.user ?? response;

      setUsers((currentUsers) =>
        currentUsers.map((candidate) =>
          candidate.id === updatedUser.id ? updatedUser : candidate
        )
      );
    } catch (err) {
      setUsers(previousUsers);
      setError(err.message || "Failed to update user.");
    } finally {
      setActionUserId("");
    }
  }

  return (
    <div className="admin-users-page">
      <section className="admin-users-panel">
        <div className="admin-users-header">
          <div>
            <p className="admin-users-eyebrow">Admin Panel</p>
            <h1>User Management</h1>
          </div>

          <form className="admin-users-search" onSubmit={handleSearchSubmit}>
            <Search size={18} />
            <input
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder="Search email or nickname"
            />
            <button type="submit" className="btn">Search</button>
          </form>
        </div>

        {loading && <div className="admin-users-state">Loading users...</div>}

        {!loading && error && <div className="admin-users-error">{error}</div>}

        {!loading && !error && users.length === 0 && (
          <div className="admin-users-state">No users found.</div>
        )}

        {!loading && users.length > 0 && (
          <>
            <div className="admin-users-table-wrap">
              <table className="admin-users-table">
                <thead>
                  <tr>
                    <th>Email</th>
                    <th>Nickname</th>
                    <th>Status</th>
                    <th>Created</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((user) => {
                    const isSelf = user.id === userId;
                    const isProtectedAdmin = user.role === "Admin" && user.isActive;
                    const disabled = isSelf || isProtectedAdmin || actionUserId === user.id;
                    const actionLabel = user.isActive ? "Ban" : "Unban";

                    return (
                      <tr key={user.id}>
                        <td data-label="Email">{user.email}</td>
                        <td data-label="Nickname">{user.nickname}</td>
                        <td data-label="Status">
                          <span className={`admin-status-badge ${user.isActive ? "active" : "banned"}`}>
                            {user.isActive ? "Active" : "Banned"}
                          </span>
                        </td>
                        <td data-label="Created">{formatCreatedAt(user.createdAt)}</td>
                        <td data-label="Action">
                          <button
                            type="button"
                            className={`admin-action-button ${user.isActive ? "ban" : "unban"}`}
                            onClick={() => handleUserAction(user)}
                            disabled={disabled}
                            title={isSelf || isProtectedAdmin ? "Protected account" : actionLabel}
                          >
                            {actionUserId === user.id ? "Saving..." : actionLabel}
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            <div className="admin-users-pagination">
              <span>
                Page {pageInfo.page} of {Math.max(pageInfo.totalPages, 1)} - {pageInfo.totalCount} users
              </span>

              <div className="admin-users-pagination-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => goToPage(pageInfo.page - 1)}
                  disabled={!pageInfo.hasPreviousPage}
                >
                  Previous
                </button>

                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => goToPage(pageInfo.page + 1)}
                  disabled={!pageInfo.hasNextPage}
                >
                  Next
                </button>
              </div>
            </div>
          </>
        )}
      </section>
    </div>
  );
}

export default AdminUsersPage;
